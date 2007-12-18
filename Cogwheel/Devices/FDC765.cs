using System;
using System.IO;
using word = System.UInt16;

namespace BeeDevelopment.Cogwheel.Devices {
    public class FDC765 {

        #region fdc765.h

        const int FDC765_MAXDRIVES = 2; // Number of emulated drives
        const int FDC765_SPT = 16; // (sectors per track) (9, max. 18 possible)
        const int FDC765_BPS = 2; // (bytes per sector) (2 for 0x200 Bytes)

        class FDC765_DiskHeader {
            public byte[] tag = new byte[0x30];     /* 00-21  MV - CPC ...                                      */
            /* 22-2F  unused (0)                                        */
            public byte nbof_tracks;   /* 30     number of tracks (40)                             */
            public byte nbof_heads;    /* 31     number of heads (1) 2 not yet supported by cpcemu */
            public short tracksize;	/*        short must be 16bit integer                       */
            /* 32-33  tracksize (including 0x100 bytes header)          */
            /*        9 sectors * 0x200 bytes each + header = 0x1300    */
            public byte[] unused = new byte[0xcc];  /* 34-FF  unused (0)                                        */

        }

        class FDC765_Track {
            public byte[] DiscData = new byte[4096];        // 16*256 bytes Data
        }

        class FDC765_Disk {
            public bool HasDisk;                // TRUE if a disk is inserted
            public FDC765_DiskHeader Header = new FDC765_DiskHeader();                 // then the structure is valid
            public FDC765_Track[] Tracks;
            public int TracksSize;
        }

        // Added by Marc Le Douarain for SF-7000 emulation ----------------------------
        // (Pin 17 of the FDC is connected to SF-7000 [PA2] input port)
        public bool FDC765_Cmd_For_SF7000;

        #endregion

        #region fdc765.c

        //-----------------------------------------------------------------------------

        FDC765_Disk[] dsk = new FDC765_Disk[FDC765_MAXDRIVES];


        //-----------------------------------------------------------------------------

        public bool FloppyMotor;                    // True= Motor ON, False= Motor OFF
        int FDCCurrDrv;                     // Current drive
        bool[] FDCWrProtect = new bool[FDC765_MAXDRIVES]; // Write protection, not used so far
        byte[] FDCCurrTrack = new byte[FDC765_MAXDRIVES]; // Current track of each drive
        byte[] FDCCurrSide = new byte[FDC765_MAXDRIVES];  // Current side of each drive
        bool ExecCmdPhase;                   // TRUE=Kommandophase findet gerade statt
        bool ResultPhase;                    // TRUE=Result-Phase findet gerade statt
        int StatusRegister;                 // Status Register
        public word StatusCounter;
        int st0, st1, st2, st3;

        byte[] FDCCommand = new byte[9];            /* Feld für Kommandos  */
        byte[] FDCResult = new byte[7];             /* Feld für Ergebnisse */

        word FDCPointer;                /* Zeiger auf die akt. Variable im Komando-Feld (beim Übertragen) */
        //word FDCCmdPointer;             /* Zeiger auf das aktuell zu übertragende Zeichen (READ/WRITE)    */
        word FDCResPointer;             /* Zeiger auf das akt. Result                                     */
        word FDCResCounter;             /* Anzahl der Results, die Zurückgegeben werden                   */
        ulong FDCDataPointer;      /* Sektor-Zeiger (Zähler) */
        ulong FDCDataLength;       /* Anzahl der zu lesenden Daten */
        word TrackIndex;                /* Index auf dsk[].Tracks[....] */
        ulong TrackDataStart;      /* Startposition der Daten des akt. Sektors im Track */

        byte[] bytes_in_cmd = {
                1,  /*  0 = none                                */
	            1,  /*  1 = none                                */
	            9,  /*  2 = READ TRACK, not implemented         */
	            3,  /*  3 = SPECIFY                             */
	            2,  /*  4 = SENSE DRIVE STATUS                  */
	            9,  /*  5 = WRITE DATA                          */
	            9,  /*  6 = READ DATA                           */
	            2,  /*  7 = RECALIBRATE                         */
	            1,  /*  8 = SENSE INTERRUPT STATUS              */
	            9,  /*  9 = WRITE DELETED DATA, not implemented */
	            2,  /* 10 = READ SECTOR ID                      */
	            1,  /* 11 = none                                */
	            9,  /* 12 = READ DELETED DATA, not implemented  */
	            6,  /* 13 = FORMAT A TRACK                      */
	            1,  /* 14 = none                                */
	            3,  /* 15 = SEEK                                */
	            1,  /* 16 = none                                */
	            9,  /* 17 = SCAN EQUAL                          */
	            1,  /* 18 = none                                */
	            1,  /* 19 = none                                */
	            1,  /* 20 = none                                */
	            1,  /* 21 = none                                */
	            1,  /* 22 = none                                */
	            1,  /* 23 = none                                */
	            1,  /* 24 = none                                */
	            9,  /* 25 = SCAN LOW OR EQUAL                   */
	            1,  /* 26 = none                                */
	            1,  /* 27 = none                                */
	            1,  /* 28 = none                                */
	            1,  /* 29 = none                                */
	            9,  /* 30 = SCAN HIGH OR EQUAL                  */
	            1   /* 31 = none                                */
        };

        /*********************************************************************/
        /**                                                                 **/
        /*********************************************************************/
        void GetRes7()                      /* Return 7 result bytes */
        {
            FDCResult[0] = (byte)st0;
            FDCResult[1] = (byte)st1;
            FDCResult[2] = (byte)st2;
            FDCResult[3] = FDCCommand[2];         /* C, H, R, N */
            FDCResult[4] = FDCCommand[3];
            FDCResult[5] = FDCCommand[4];
            FDCResult[6] = FDCCommand[5];
            StatusRegister = 0xD0;                /* Ready to return results */
            StatusCounter = 100;
            FDCResPointer = 0;
            FDCResCounter = 7;
            st0 = st1 = st2 = 0;
            ExecCmdPhase = false;
            ResultPhase = true;
        }

        /*********************************************************************/
        /**                                                                 **/
        /*********************************************************************/
        void FDCExecWriteCommand(byte Value) {
            switch (FDCCommand[0]) {
                case 2:             /* Read track */
                    FDCCurrDrv = FDCCommand[1] & 3;
                    FDCCurrSide[FDCCurrDrv] = (byte)((FDCCommand[1] >> 2) & 1);
                    FDCCurrTrack[FDCCurrDrv] = FDCCommand[2];
                    if (dsk[FDCCurrDrv].HasDisk == false) {
                        st0 = FDCCurrDrv | 0xD8;  /* Equipment check, Not ready */
                        GetRes7();
                    } else {
                        FDCCurrSide[FDCCurrDrv] = (byte)((FDCCommand[1] >> 2) & 1);
                        FDCCurrTrack[FDCCurrDrv] = FDCCommand[2];
                        ExecCmdPhase = true;
                        TrackIndex = (ushort)(FDCCurrTrack[FDCCurrDrv] * dsk[FDCCurrDrv].Header.nbof_heads + FDCCurrSide[FDCCurrDrv]);
                        TrackDataStart = (ulong)(((FDCCommand[4] & 0x0F) - 1) << 9);
                        //        FDCDataLength = (dsk[FDCCurrDrv].Tracks[TrackIndex].BPS * dsk[FDCCurrDrv].Tracks[TrackIndex].SPT) << 9;
                        FDCDataLength = (FDC765_BPS * FDC765_SPT) << 9;
                        FDCDataPointer = 0;
                        StatusCounter = 100;
                        StatusRegister = 0xF0;     /* RQM=1, DIO=FDC->CPU, EXM=1, CB=1 */
                    }
                    break;

                case 3:             /* Specify */
                    break;

                case 4:             /* Sense drive status */
                    FDCCurrDrv = FDCCommand[1] & 3;
                    st3 = FDCCommand[1] & 7;
                    if (FDCWrProtect[FDCCurrDrv]) st3 |= 0x40;
                    if (dsk[FDCCurrDrv].HasDisk) st3 |= 0x20;
                    if (FDCCurrTrack[FDCCurrDrv] == 0) st3 |= 0x10;
                    if ((st3 & 4) != 0) st3 |= 8; /* Two side drive */
                    FDCResCounter = 1;       /* Ein Result-Byte, das zurück gegeben wird */
                    FDCResPointer = 0;
                    FDCResult[0] = (byte)st3;
                    ExecCmdPhase = false;
                    ResultPhase = true;
                    StatusCounter = 100;
                    StatusRegister = 0xD0;		    /* Ready to return results */
                    break;

                case 5:              /* Write data */
                    if (!ExecCmdPhase) {
                        FDCCurrDrv = FDCCommand[1] & 3;
                        FDCCurrSide[FDCCurrDrv] = (byte)((FDCCommand[1] >> 2) & 1);
                        FDCCurrTrack[FDCCurrDrv] = FDCCommand[2];
                        ExecCmdPhase = true;
                        if (dsk[FDCCurrDrv].HasDisk == false) {
                            st0 = FDCCurrDrv | 0xD8;  /* Equipment check, Not ready */
                            GetRes7();
                        } else {
                            TrackIndex = (ushort)(FDCCurrTrack[FDCCurrDrv] * dsk[FDCCurrDrv].Header.nbof_heads + FDCCurrSide[FDCCurrDrv]);
                            TrackDataStart = (ulong)(((FDCCommand[4] & 0x0F) - 1) << 9);
                            FDCDataLength = (ulong)(512 + ((FDCCommand[4] - FDCCommand[6]) << 9));
                            FDCDataPointer = 0;
                            StatusCounter = 100;
                            StatusRegister = 0xB0;     /* RQM=1, DIO=CPU->FDC, EXM=1, CB=1 */
                        }
                    } else {
                        dsk[FDCCurrDrv].Tracks[TrackIndex].DiscData[TrackDataStart + FDCDataPointer] = Value;
                        FDCDataPointer++;
                        if (FDCDataPointer == FDCDataLength) {
                            st0 = FDCCommand[1] & 7;
                            GetRes7();
                        }
                    }
                    break;

                case 6:                      /* Read data */
                    FDCCurrDrv = FDCCommand[1] & 3;
                    FDCCurrSide[FDCCurrDrv] = (byte)((FDCCommand[1] >> 2) & 1);
                    FDCCurrTrack[FDCCurrDrv] = FDCCommand[2];
                    if (dsk[FDCCurrDrv].HasDisk == false) {
                        st0 = FDCCurrDrv | 0xD8;  /* Equipment check, Not ready */
                        GetRes7();
                    } else {
                        ExecCmdPhase = true;
                        TrackIndex = (ushort)(FDCCurrTrack[FDCCurrDrv] * dsk[FDCCurrDrv].Header.nbof_heads + FDCCurrSide[FDCCurrDrv]);
                        //        TrackDataStart = ((FDCCommand[4] & 0x0F)-1) << 9;
                        //        FDCDataLength = 512 + (((FDCCommand[4] & 0xF) - (FDCCommand[6] & 0xF))<<9);
                        TrackDataStart = (ulong)(((FDCCommand[4] & 0x1F) - 1) << 8);
                        FDCDataLength = TrackDataStart + 256;// + (((FDCCommand[4] & 0xF) - (FDCCommand[6] & 0xF))<<8);
                        FDCDataPointer = 0;
                        StatusCounter = 100;
                        StatusRegister = 0xF0; /* RQM=1, DIO=FDC->CPU, EXM=1, CB=1 */
                    }
                    break;

                case 7:                     /* Recalibrate (Track 0 Lookup) */
                    st0 = st1 = st2 = 0;
                    FDCCurrDrv = FDCCommand[1] & 3;
                    st0 = FDCCommand[1] & 7;
                    if (dsk[FDCCurrDrv].HasDisk == false) {
                        st0 |= 0xD8;  /* Equipment check, Not ready */
                    } else {
                        if (FDCCurrTrack[FDCCurrDrv] > 77) {
                            FDCCurrTrack[FDCCurrDrv] -= 77;
                            st0 |= 0x30;
                        } else {
                            FDCCurrTrack[FDCCurrDrv] = 0;
                            st0 |= 0x20;
                        }
                    }
                    StatusCounter = 100;
                    StatusRegister = 0x80 | (1 << (FDCCommand[1] & 3)); /* RQM=1, DIO=CPU->FDC, EXM = 0 */
                    ExecCmdPhase = false;
                    break;

                case 8:                    /* Sense Interrupt */
                    StatusRegister = 0xD0;   /* RQM=1, DIO=FDC->CPU, EXM = 0, CB=1, DB0-DB3 = 0 */
                    FDCResCounter = 2;       /* Two Result-Bytes, die zurück gegeben werden */
                    FDCResPointer = 0;
                    //st0 = FDCCurrDrv | (FDCCurrSide[FDCCurrDrv]<<2);
                    if (dsk[FDCCurrDrv].HasDisk == false)
                        st0 |= 0x08;                  /* Drive not ready */
                    if ((st0 & 0x38) == 0) st0 |= 0x80;  /* If no interrupt is available */
                    /* MLD */
                    // Needed else SF-7000 IPL says 'Cannot read this disk'
                    st0 &= 0x3F;
                    FDCResult[0] = (byte)st0; st0 = 0x00;
                    FDCResult[1] = FDCCurrTrack[FDCCurrDrv];
                    ExecCmdPhase = false;
                    ResultPhase = true;
                    StatusCounter = 100;
                    break;

                case 10:                   /* ID des nächsten Sektors lesen */
                    FDCCurrDrv = FDCCommand[1] & 3;
                    FDCCurrSide[FDCCurrDrv] = (byte)((FDCCommand[1] >> 2) & 1);
                    if (dsk[FDCCurrDrv].HasDisk == false) {
                        st0 = FDCCurrDrv | 0xD8;  /* Equipment check, Not ready */
                        GetRes7();
                    } else {
                        TrackIndex = (ushort)(FDCCurrTrack[FDCCurrDrv] * dsk[FDCCurrDrv].Header.nbof_heads + FDCCurrSide[FDCCurrDrv]);
                        st0 = FDCCommand[1] & 7;
                        GetRes7();
                        //        FDCResult[5] = dsk[FDCCurrDrv].Tracks[TrackIndex].sector[0].sector;   /* 0x01=IBM, 0x41=Data, 0xC1=System */
                        FDCResult[5] = 0x41;   /* 0x01=IBM, 0x41=Data, 0xC1=System */
                    }
                    break;

                case 15:                    /* SEEK - Spur suchen */
                    StatusCounter = 100;
                    StatusRegister = 0x80 | (1 << (FDCCommand[1] & 3));
                    FDCCurrDrv = FDCCommand[1] & 3;
                    FDCCurrSide[FDCCurrDrv] = (byte)((FDCCommand[1] >> 2) & 1);
                    if (dsk[FDCCurrDrv].HasDisk == false) {
                        st0 = FDCCurrDrv | 0xD8;  /* Equipment check, Not ready */
                        GetRes7();
                    } else {
                        FDCCurrTrack[FDCCurrDrv] = FDCCommand[2];
                        /* Diskette eingelegt? */
                        if (dsk[FDCCurrDrv].HasDisk == true)
                            st0 = 0x20 | (FDCCommand[1] & 7); /* SEEK end + HD + US1 + US0 */
                        else
                            st0 = 0x08 | (FDCCommand[1] & 7); /* NOT READY + HD + US1 + US0 */
                        ExecCmdPhase = false;
                    }
                    break;

                default:
                    //Msg(MSGT_DEBUG, Msg_Get(MSG_FDC765_Unknown_Write), FDCCommand[0]);
                    break;
            }
        }

        /*********************************************************************/
        /**                                                                 **/
        /*********************************************************************/
        byte FDCExecReadCommand() {
            byte ret = 0;

            switch (FDCCommand[0]) {
                case 2:
                    ret = dsk[FDCCurrDrv].Tracks[TrackIndex].DiscData[TrackDataStart + FDCDataPointer];
                    FDCDataPointer++;
                    if (FDCDataPointer == FDCDataLength) {
                        st0 = (FDCCommand[1] & 7) | 0x40;   /* Unit, head, command canceled */
                        st1 = 0x80;                         /* End of track error           */
                        GetRes7();
                    }
                    break;

                case 6:
                    ret = dsk[FDCCurrDrv].Tracks[TrackIndex].DiscData[TrackDataStart + FDCDataPointer];
                    FDCDataPointer++;
                    if (FDCDataPointer == FDCDataLength) {
                        st0 = (FDCCommand[1] & 7) | 0x40;   /* Unit, head, command canceled */
                        st1 = 0x80;                         /* End of track error           */
                        GetRes7();
                    }
                    break;

                default:
                    //Msg(MSGT_DEBUG, Msg_Get(MSG_FDC765_Unknown_Read), FDCCommand[0]);
                    break;
            }
            return ret;
        }

        /*********************************************************************/
        /**                                                                 **/
        /*********************************************************************/
        byte FDCGetResult() {
            byte ret = FDCResult[FDCResPointer];

            FDCResPointer++;
            if (FDCResPointer == FDCResCounter) {
                StatusRegister = 0x80;
                ResultPhase = false;
            }
            return ret;
        }

        /*********************************************************************/
        /**                                                                 **/
        /*********************************************************************/
        void FDC765_Init() {
            int i;


            for (i = 0; i < FDC765_MAXDRIVES /* was 1 ?!? */; i++) {
                dsk[i] = new FDC765_Disk();
                dsk[i].HasDisk = false;
                dsk[i].Tracks = null;
                dsk[i].TracksSize = 0;
            }
            FDC765_Reset();
        }

        // A constructor is rather more OOP.
        public FDC765() {
            FDC765_Init();
        }

        /*********************************************************************/
        /**                                                                 **/
        /*********************************************************************/
        void FDC765_Close() {
            int i;

            for (i = 0; i < FDC765_MAXDRIVES /* was 1 ?!? */; i++) {
                // WriteDskImage (i);
                //free(dsk[i].Tracks); // free() is doing the NULL test
            }
        }

        /*********************************************************************/
        /**                                                                 **/
        /*********************************************************************/
        public void FDC765_Reset() {
            int i;

            FloppyMotor = false;
            FDCPointer = 0;
            ExecCmdPhase = false;
            ResultPhase = false;
            StatusRegister = 128;

            for (i = 0; i < FDC765_MAXDRIVES; i++) {
                FDCCurrTrack[i] = 0;
                FDCWrProtect[i] = false;
            }
            for (i = 0; i < 9; i++)
                FDCCommand[i] = 0;
            FDC765_Cmd_For_SF7000 = false;
        }


        /*********************************************************************/
        /**                                                                 **/
        /*********************************************************************/
        public void FDC765_Data_Write(byte Value) {
            Console.WriteLine("Writing data: {0:X2}.", Value);
            if (!ExecCmdPhase) {
                if (FDCPointer == 0) {
                    FDCCommand[0] = (byte)(Value & 0x1F);  /* New Command */
                    FDCPointer++;
                    StatusRegister |= 0x10;         /* FDC Busy */
                } else
                    if (FDCPointer < bytes_in_cmd[FDCCommand[0]]) {
                        FDCCommand[FDCPointer] = Value; // Parameter for the command
                        FDCPointer++;
                    }

                if (FDCPointer == bytes_in_cmd[FDCCommand[0]]) {
                    FDCPointer = 0;
                    StatusRegister |= 0x20;
                    FDCExecWriteCommand(Value);                     /* Kommando ausführen */
                    FDC765_Cmd_For_SF7000 = true;
                }
            } else {
                FDCExecWriteCommand(Value);                     /* Kommando ausführen */
            }
        }

        /*********************************************************************/
        /**                                                                 **/
        /*********************************************************************/
        public byte FDC765_Data_Read() {
            Console.WriteLine("Reading data.");
            FDC765_Cmd_For_SF7000 = false;
            if (ExecCmdPhase)
                return FDCExecReadCommand();
            if (ResultPhase)
                return FDCGetResult();
            return 0;
        }

        /*********************************************************************/
        /**                                                                 **/
        /*********************************************************************/
        public byte FDC765_Status_Read() {
            Console.WriteLine("Reading status.");
            // if (StatusCounter > 0)
            //    {
            //    StatusCounter --;
            //    return 0;
            //    }
            return (byte)StatusRegister;
        }

        /*********************************************************************/
        /**                                                                 **/
        /*********************************************************************/
        /*void FDC765_Disk_Write_Get(int DrvNum, void** Data, int* DataSize) {
            if (dsk[DrvNum].HasDisk) {
                *Data = dsk[DrvNum].Tracks;
                *DataSize = dsk[DrvNum].TracksSize;
            } else {
                *Data = NULL;
                *DataSize = 0;
            }
        }*/

        public void FDC765_Disk_Remove(int DrvNum) {
            dsk[DrvNum].HasDisk = false;
            dsk[DrvNum].Tracks = null;
        }

        /*********************************************************************/
        /**                                                                 **/
        /** Zeigt einen Datei-Dialog zur Auswahl von Disk-Image-Dateien an, **/
        /** schließt eine evtl. bereits geöffnete Datei, öffnet die ausge-  **/
        /** wählte Image-Datei und liest diese in den Speicher ein.         **/
        /** Der für die Track-Informationen und die Daten des Disk-Images   **/
        /** benötigte Speicher wird jedoch nur einmal mit MALLOC vom Syste  **/
        /** angefordert und immer wieder verwendet, bis die Emulation be-   **/
        /** endet wird.                                                     **/
        /**                                                                 **/
        /** DRVNUM = Driver Number (0 for A: and 1 for B:)                  **/
        /**                                                                 **/
        /*********************************************************************/

        public void FDC765_Disk_Insert(int DrvNum, Stream Data, int DataSize) {
            FDC765_Disk Disk = dsk[DrvNum];

            // Write existing disk ?
            // WriteDskImage (DrvNum);

            // Remove existing disk
            FDC765_Disk_Remove(DrvNum);

            // Set HasDisk and Write Protection flags
            Disk.HasDisk = true;
            FDCWrProtect[DrvNum] = true; // Write protection always ON yet

            // No header in sf7000 image disks, initialization here
            Disk.Header.nbof_tracks = 40;
            Disk.Header.nbof_heads = 1;
            Disk.Header.tracksize = (16 * 0x100);

            // Calculating track size and allocating memory for it
            Disk.TracksSize = Disk.Header.tracksize * Disk.Header.nbof_tracks * Disk.Header.nbof_heads;
            Disk.Tracks = new FDC765_Track[Disk.Header.nbof_tracks];

            for (int i = 0; i < Disk.Tracks.Length; ++i) {
                Disk.Tracks[i] = new FDC765_Track();
                Disk.Tracks[i].DiscData = new byte[Disk.Header.tracksize];
                Data.Read(Disk.Tracks[i].DiscData, 0, Disk.Header.tracksize);                
            }

            // Copying memory from data source

            /*memcpy(Disk->Tracks, Data, DataSize);
            if (DataSize > Disk->TracksSize) {
                Msg(MSGT_USER, Msg_Get(MSG_FDC765_Disk_Too_Large1), DataSize, Disk->TracksSize);
                Msg(MSGT_USER_BOX, Msg_Get(MSG_FDC765_Disk_Too_Large2));
            }
            if (DataSize < Disk->TracksSize) {
                Msg(MSGT_USER, Msg_Get(MSG_FDC765_Disk_Too_Small1), DataSize, Disk->TracksSize);
                Msg(MSGT_USER_BOX, Msg_Get(MSG_FDC765_Disk_Too_Small2));
                memset((byte*)Disk->Tracks + DataSize, 0, Disk->TracksSize - DataSize);
            }*/
        }

        //-----------------------------------------------------------------------------

        #endregion

    }
}
