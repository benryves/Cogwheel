using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace BeeDevelopment.Cogwheel.Silverlight {

	public static class RenderLoop {

		public static void AttachRenderLoop(this FrameworkElement c, Action update) {
			var Board = new Storyboard();
			c.Resources.Add("RenderLoop", Board);
			Board.Completed += (sender, e) => {
				if (update != null) update();
				Board.Begin();
			};
			Board.Begin();
		}

		public static void DetachRenderLoop(this FrameworkElement c) {
			var Board = (Storyboard)c.Resources["RenderLoop"];
			Board.Stop();
			c.Resources.Remove("RenderLoop");
		}
	
	}
}
