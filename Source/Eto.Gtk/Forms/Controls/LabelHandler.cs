using System;
using Eto.Forms;
using Eto.Drawing;
using Eto.GtkSharp.Drawing;

namespace Eto.GtkSharp.Forms.Controls
{
	public class LabelHandler : GtkControl<LabelHandler.EtoLabel, Label, Label.ICallback>, Label.IHandler
	{
		readonly Gtk.EventBox eventBox;
		TextAlignment horizontalAlign = TextAlignment.Left;
		VerticalAlignment verticalAlign = VerticalAlignment.Top;

		public override Gtk.Widget ContainerControl
		{
			get { return eventBox; }
		}

		public override Gtk.Widget EventControl
		{
			get { return eventBox; }
		}

		public class EtoLabel : Gtk.Label
		{
			int? wrapWidth;

			public void ResetWidth()
			{
				wrapWidth = null;
			}

			#if GTK2
			protected override void OnSizeRequested(ref Gtk.Requisition requisition)
			{
				int width, height;
				Layout.GetPixelSize(out width, out height);
				requisition.Width = width;
				requisition.Height = height;
			}
			#else

			protected override void OnAdjustSizeRequest (Gtk.Orientation orientation, out int minimum_size, out int natural_size)
			{
				base.OnAdjustSizeRequest (orientation, out minimum_size, out natural_size);
				if (orientation == Gtk.Orientation.Horizontal)
				{
					minimum_size = natural_size;// wrapWidth ?? natural_size;
				}
			}
			#endif

			protected override void OnSizeAllocated(Gdk.Rectangle allocation)
			{
				base.OnSizeAllocated(allocation);
				SetWrapWidth(allocation.Width);
			}

			void SetWrapWidth(int width)
			{
				if (!IsRealized || SingleLineMode || width == 0)
				{
					HeightRequest = -1;
					wrapWidth = null;
					return;
				}
				if (wrapWidth != width)
				{
					Layout.Width = (int)(width * Pango.Scale.PangoScale);
					int pixWidth, pixHeight;
					Layout.GetPixelSize(out pixWidth, out pixHeight);
					HeightRequest = pixHeight;
					wrapWidth = width;
					#if GTK3
					if (Parent != null)
						Gtk.Application.Invoke((sender, e) => Parent.QueueResize());
					#endif
				}
			}
		}

		public LabelHandler()
		{
			eventBox = new Gtk.EventBox();
			eventBox.ResizeMode = Gtk.ResizeMode.Immediate;
			//eventBox.VisibleWindow = false;
			Control = new EtoLabel();
			Control.SetAlignment(0, 0);
			eventBox.Child = Control;
			Wrap = WrapMode.Word;
		}

		public WrapMode Wrap
		{
			get
			{
				if (!Control.LineWrap)
					return WrapMode.None;
				if (Control.LineWrapMode == Pango.WrapMode.Char)
					return WrapMode.Character;
				return WrapMode.Word;
			}
			set
			{
				Control.ResetWidth();
				switch (value)
				{
					case WrapMode.None:
						Control.Wrap = false;
						Control.LineWrap = false;
						Control.SingleLineMode = true;
						break;
					case WrapMode.Word:
						Control.Wrap = true;
						Control.Layout.Wrap = Pango.WrapMode.WordChar;
						Control.LineWrapMode = Pango.WrapMode.WordChar;
						Control.LineWrap = true;
						Control.SingleLineMode = false;
						break;
					case WrapMode.Character:
						Control.Wrap = true;
						Control.Layout.Wrap = Pango.WrapMode.Char;
						Control.LineWrapMode = Pango.WrapMode.Char;
						Control.LineWrap = true;
						Control.SingleLineMode = false;
						break;
					default:
						throw new NotSupportedException();
				}
				eventBox.QueueResize();
			}
		}

		public override void AttachEvent(string id)
		{
			switch (id)
			{
				case TextControl.TextChangedEvent:
					break;
				default:
					base.AttachEvent(id);
					break;
			}
		}

		public virtual Color TextColor
		{
			get { return Control.GetForeground(); }
			set { Control.SetForeground(value); }
		}

		public override string Text
		{
			get { return Control.Text.ToEtoMnemonic(); }
			set
			{
				Control.ResetWidth();
				Control.TextWithMnemonic = value.ToPlatformMnemonic();
			}
		}

		public TextAlignment TextAlignment
		{
			get { return horizontalAlign; }
			set
			{
				horizontalAlign = value;
				SetAlignment();
			}
		}

		void SetAlignment()
		{
			float xalignment;
			float yalignment;
			switch (horizontalAlign)
			{
				default:
					xalignment = 0F;
					break;
				case TextAlignment.Center:
					xalignment = 0.5F;
					break;
				case TextAlignment.Right:
					xalignment = 1F;
					break;
			}
			switch (verticalAlign)
			{
				case VerticalAlignment.Center:
					yalignment = 0.5F;
					break;
				default:
					yalignment = 0F;
					break;
				case VerticalAlignment.Bottom:
					yalignment = 1F;
					break;
			}
			Control.SetAlignment(xalignment, yalignment);
			Control.Justify = horizontalAlign.ToGtk();
		}

		public VerticalAlignment VerticalAlignment
		{
			get { return verticalAlign; }
			set
			{
				verticalAlign = value;
				SetAlignment();
			}
		}

		public override Font Font
		{
			get { return base.Font; }
			set
			{
				Control.ResetWidth();
				base.Font = value;
				Control.Attributes = value != null ? ((FontHandler)value.Handler).Attributes : null;
			}
		}
	}
}
