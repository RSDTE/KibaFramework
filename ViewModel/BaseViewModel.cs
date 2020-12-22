using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ViewModel.Annotations;

namespace ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public const string UINameSapce = "KibaFramework";
        public string UIElementName = string.Empty;
        public FrameworkElement UIElement { get; set; }
        public Window windowMain { get; set; }

        public EventHandler CloseCallBack = null;

        public BaseViewModel()
        {
            windowMain = Application.Current.MainWindow;
            SetUIElement();
        }

        public void SetUIElement()
        {
            Type childType = this.GetType();
            string name = this.GetType().Name;
            UIElementName = name.Replace("VM_", "");
            UIElementName = UIElementName.Replace("`1", "");

            if (name.Contains("Window"))
            {
                UIElement = GetElement<Window>();
                (UIElement as Window).Closing += (s, e) => { CloseCallBack?.Invoke(s, e); };
            }
            else if (name.Contains("Page"))
            {
                UIElement = GetElement<Page>();
                (UIElement as Page).Unloaded += (s, e) => CloseCallBack?.Invoke(s, e);
            }
            else if (name.Contains("UC"))
            {
                UIElement = GetElement<UserControl>();
                (UIElement as UserControl).Unloaded += (s, e) => CloseCallBack?.Invoke(s, e);
            }
            else
            {
                throw new Exception("元素名不规范");
            }
        }

        public E GetElement<E>()
        {
            Type type = GetFromType(UINameSapce + "." + UIElementName);
            E element = (E)Activator.CreateInstance(type);
            return element;
        }

        public static Type GetFromType(string fullName)
        {
            Assembly assembly = Assembly.Load(UINameSapce);
            Type type = assembly.GetType(fullName, true, false);
            return type;
        }

        public void Show()
        {
            if (UIElement is Window win)
            {
                win.Show();
                return;
            }

            throw new Exception("元素类型不正确");
        }

        public void ShowDialog()
        {
            if (UIElement is Window win)
            {
                win.ShowDialog();
                return;
            }

            throw new Exception("元素类型不正确");
        }

        public void Close()
        {
            if (UIElement is Window win)
            {
                win.Close();
                return;
            }

            throw new Exception("元素类型不正确");
        }

        public void Hide()
        {
            if (UIElement is Window win)
            {
                win.Hide();
                return;
            }

            throw new Exception("元素类型不正确");
        }

        public void MessageBox(Window owner, string msg)
        {
            DispatcherHelper.GetUIDispatcher().Invoke(new Action(() =>
            {
                if (owner is null)
                {
                    System.Windows.MessageBox.Show(windowMain, msg, "提示信息");
                }
                else
                {
                    System.Windows.MessageBox.Show(owner, msg, "提示信息");
                }
            }));
        }

        public void MessageBox(string msg)
        {
            DispatcherHelper.GetUIDispatcher().Invoke(new Action(() =>
            {
                System.Windows.MessageBox.Show(windowMain, msg, "提示信息");
            }));
        }

        public void MessageBox(string msg, string strTitle)
        {
            DispatcherHelper.GetUIDispatcher().Invoke(new Action(() =>
            {
                System.Windows.MessageBox.Show(windowMain, msg, "提示信息");
            }));
        }

        public void MessageBox(string title, string msg, Action<bool> callback)
        {
            DispatcherHelper.GetUIDispatcher().Invoke(new Action(() =>
            {
                if (System.Windows.MessageBox.Show(windowMain, msg, title, MessageBoxButton.YesNo) ==
                    MessageBoxResult.Yes)
                {
                    callback(true);
                }
                else
                {
                    callback(false);
                }
            }));
        }

        public void MessageBox(string msg, Action<bool> callback)
        {
            MessageBox("系统提示", msg, callback);
        }

        public void AsyncLoad(Action action)
        {
            IAsyncResult result = action.BeginInvoke((iar) => { }, null);
        }

        public void AsyncLoad(Action action, Action callback)
        {
            IAsyncResult result = action.BeginInvoke((iar) => { this.DoMenthodByDispatcher(callback); }, null);
        }

        public void AsyncLoad<T>(Action<T> action, T para, Action callback)
        {
            IAsyncResult result = action.BeginInvoke(para, (iar) => { this.DoMenthodByDispatcher(callback); }, null);
        }

        public void AsyncLoad<T, R>(Func<T, R> action, T para, Action<R> callback)
        {
            IAsyncResult result = action.BeginInvoke(para, (iar) =>
            {
                var res = action.EndInvoke(iar);
                this.DoMenthodByDispatcher<R>(callback, res);
            }, null);
        }

        public void AsyncLoad<R>(Func<R> action, Action<R> callback)
        {
            IAsyncResult result = action.BeginInvoke((iar) =>
            {
                var res = action.EndInvoke(iar);
                this.DoMenthodByDispatcher<R>(callback, res);
            }, null);
        }

        public void DoMenthodByDispatcher<T>(Action<T> action, T obj)
        {
            DispatcherHelper.GetUIDispatcher()
                .BeginInvoke(new Action(() => { action(obj); }), DispatcherPriority.Normal);
        }

        public void DoMenthodByDispatcher(Action action)
        {
            DispatcherHelper.GetUIDispatcher().BeginInvoke(new Action(() => { action(); }), DispatcherPriority.Normal);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}