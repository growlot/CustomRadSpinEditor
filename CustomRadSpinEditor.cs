using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace CustomRadSpinEditor
{
    public class CustomRadSpinEditor : RadSpinEditor
    {
        private event EventHandler<CustomRadSpinEelemntTextRequiredEventArgs> _textRequired;
        private event EventHandler<KeyPressEventArgs> _validateKeyPress;

        public event EventHandler<CustomRadSpinEelemntTextRequiredEventArgs> TextRequired
        {
            add
            {
                lock (this)
                {
                    _textRequired += value;
                }
            }
            remove
            {
                lock (this)
                {
                    _textRequired -= value;
                }
            }
        }

        public event EventHandler<KeyPressEventArgs> ValidateKeyPress
        {
            add
            {
                lock (this)
                {
                    _validateKeyPress += value;
                }
            }
            remove
            {
                lock (this)
                {
                    _validateKeyPress -= value;
                }
            }
        }

        public CustomRadSpinEditor()
            :base()
        {
            this.AutoSize = true;
            this.TabStop = false;
            base.SetStyle(ControlStyles.Selectable, true);
        }

        protected override void CreateChildItems(RadElement parent)
        {
            Type baseType = typeof(RadSpinEditor);
            CustomRadSpinElement element = new CustomRadSpinElement();
            element.RightToLeft = this.RightToLeft == System.Windows.Forms.RightToLeft.Yes;
            this.RootElement.Children.Add(element);

            element.ValueChanging += spinElement_ValueChanging;
            element.ValueChanged += spinElement_ValueChanged;
            element.TextChanging += spinElement_TextChanging;

            element.KeyDown += OnSpinElementKeyDown;
            element.KeyPress += OnSpinElementKeyPress;
            element.KeyUp += OnSpinElementKeyUp;

            element.TextRequired += spinElement_TextRequired;
            element.ValidateKeyPress += spinElement_ValidateKeyPress;

            baseType.GetField("spinElement", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, element);
        }

        private void spinElement_ValidateKeyPress(object sender, KeyPressEventArgs e)
        {
            _validateKeyPress?.Invoke(sender, e);
        }

        private void spinElement_TextRequired(object sender, CustomRadSpinEelemntTextRequiredEventArgs e)
        {
            _textRequired?.Invoke(sender, e);
        }

        private Dictionary<string, MethodInfo> cache = new Dictionary<string, MethodInfo>();

        private void InvokeBaseMethod(string name, params object[] parameters)
        {
            if (!cache.ContainsKey(name))
            {
                cache[name] = typeof(RadSpinEditor).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
            }

            cache[name].Invoke(this, parameters);
        }

        private void OnSpinElementKeyUp(object sender, KeyEventArgs e)
        {
            this.InvokeBaseMethod("OnSpinElementKeyUp", sender, e);
        }

        private void OnSpinElementKeyPress(object sender, KeyPressEventArgs e)
        {
            this.InvokeBaseMethod("OnSpinElementKeyPress", sender, e);
        }

        private void OnSpinElementKeyDown(object sender, KeyEventArgs e)
        {
            this.InvokeBaseMethod("OnSpinElementKeyDown", sender, e);
        }

        private void spinElement_TextChanging(object sender, TextChangingEventArgs e)
        {
            this.InvokeBaseMethod("spinElement_TextChanging", sender, e);
        }

        private void spinElement_ValueChanged(object sender, EventArgs e)
        {
            this.InvokeBaseMethod("spinElement_ValueChanged", sender, e);
        }

        private void spinElement_ValueChanging(object sender, ValueChangingEventArgs e)
        {
            this.InvokeBaseMethod("spinElement_ValueChanging", sender, e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override Size DefaultSize
        {
            get
            {
                return GetDpiScaledSize(new Size(100, 20));
            }
        }
    }

    public class CustomRadSpinElement : RadSpinElement
    {
        public event EventHandler<CustomRadSpinEelemntTextRequiredEventArgs> TextRequired;
        public event EventHandler<KeyPressEventArgs> ValidateKeyPress;

        protected override void ValidateOnKeyPress(KeyPressEventArgs e)
        {
            ValidateKeyPress?.Invoke(this, e);
            if (!e.Handled)
                base.ValidateOnKeyPress(e);
        }

        protected override string GetNumberText(decimal num)
        {
            CustomRadSpinEelemntTextRequiredEventArgs e = new CustomRadSpinEelemntTextRequiredEventArgs()
            {
                Handled = false,
                Value = num
            };
            TextRequired?.Invoke(this, e);

            if (e.Handled)
            {
                return e.Text;
            }
            return base.GetNumberText(num);
        }

        protected override Type ThemeEffectiveType
        {
            get
            {
                return typeof(RadSpinElement);
            }
        }
    }

    public class CustomRadSpinEelemntTextRequiredEventArgs : EventArgs
    {
        public bool Handled { get; set; }
        public string Text { get; set; }
        public decimal Value { get; set; }
    }
}
