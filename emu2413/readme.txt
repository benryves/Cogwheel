The copy of emu2413 used here has been copied from the MSXplug package.
The readme.txt for MSXplug is listed below.

MSXplug ソースアーカイブ

////////////////////////////////////////////////////////////////////////
// コンパイルを確認した環境
////////////////////////////////////////////////////////////////////////

  Visual C++.NET (2003)

////////////////////////////////////////////////////////////////////////
// コンパイル方法
////////////////////////////////////////////////////////////////////////

1) zlibの配置

  コンパイルにはzlibが必要です．.\zlib\フォルダの下に，zlib 1.2.1のソー
  スアーカイブを展開してください．
  
  zipサポートが不要な場合，プロジェクトの設定でマクロKSS_ZIP_SUPPORTの
  定義を削除してください．zlib非搭載版のMSXplug を作成できます．

2) mgsdrv.hとkinrou5.h

  libkss\convert\driver\フォルダにヘッダファイルmgsdrv.hとkinrou.hがそ
  れぞれ必要です．

  これらのヘッダファイルはbin2c.exeを利用して作成できます．bin2c.exeは
  同梱のbin2cプロジェクトをコンパイルして作成してください．mgsdrv.com,
  kinrou5.drvとbin2c.exeを同じフォルダに配置し，コマンドプロンプトから，
  
  bin2c mgsdrv.com > mgsdrv.h
  bin2c kinrou5.drv > kinrou5.h

  を実行すると各ヘッダファイルが生成されます．

3) カスタムビルドの設定

  デフォルトでは，プラグイン本体が
　C:\Program Files\KbMediaPlayer\OK\in_msx\in_msx.kpi
　C:\Program Files\Winamp\Plugins\in_msx.dll
  にコピーされる設定になっています．不都合があれば適宜カスタムビルドの
  設定を書き換えてください．

4) コンパイル

  in_msxをアクティブプロジェクトに選択してコンパイルしてください．

////////////////////////////////////////////////////////////////////////
// 著作権等
////////////////////////////////////////////////////////////////////////
  
in_msx\winampフォルダ配下のFRONTEND.H, IN2.HおよびOUT.HはNullsoft, Inc.
の著作物です．最新のファイルはhttp://www.winamp.comの開発者サイトから入手
できます.

kssplay\convert\mbm2kssh.h は Mamiya氏が作成されたソースです．
  
kmz80フォルダ配下のファイルはすべてMamiya氏が作成されたものです．著作権
についてはkmz80\kmz80.txtをお読みください．

kbmediaフォルダ配下のファイルはKobarin氏が作成されたプラグインのソース
を元に，拡張を施したものです．

その他のファイル(brezza@dsa.sakura.ne.jpの記述したファイル)については，全
てフリーです．商用・非商用を問わず適当に使って頂いてかまいません．使用
連絡や、ライセンス表示の義務は一切ありません。

////////////////////////////////////////////////////////////////////////
// 謝辞
////////////////////////////////////////////////////////////////////////

MSXplugの実装にあたっては以下の資料，文献，製作物を参考にさせて頂きま
した．各著作者の皆様に感謝申し上げます．

NEZplug by Mamiya http://nezplug.sourceforge.net/ 
MBM2KSS by Mamiya http://nezplug.sourceforge.net/
MGSDRV by GIGAMIX/Ain http://www.gigamixonline.com/mgsdrv/ 
KINROU5 by Keiichi Kuroda http://www.interq.or.jp/white/white/fsw99/music.html 
MPK by K-KAZ http://mu.ice.uec.ac.jp/~kokubun/xray/mpk.html 
OPX4KSS by Mikasen http://mika1000.tripod.co.jp/ 
MPK2KSS by Naruto http://http://suzuka.cool.ne.jp/hellokss/ 
The Best Game Music!!! 5th http://nesmusic.zophar.net/top.html 
fmpac.ill by Naruto 
vrc7.ill by Fukui 
fmopl.c by Tatsuyuki Satoh 
fmgen.cpp by cisc 
MSX-Datapack (C)ASCII
emu76489 fixed by RuRuRu