﻿using FzLib.Basic;
using FzLib.Extension;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ClassifyFiles.Data;
using ClassifyFiles.Util;
using ClassifyFiles.UI;
using System.Windows.Markup;
using System.Diagnostics;
using ClassifyFiles.UI.Event;
using ClassifyFiles.UI.Model;

namespace ClassifyFiles.UI.Page
{
    public partial class ClassSettingPanel : ProjectPageBase
    {
        public ClassSettingPanel()
        {
            InitializeComponent();
        }

        public ObservableCollection<MatchCondition> matchConditions;

        public ObservableCollection<MatchCondition> MatchConditions
        {
            get => matchConditions;
            set
            {
                matchConditions = value;
                this.Notify(nameof(MatchConditions));
            }
        }
        private void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            //不知道为什么，Xaml里绑定不上，只好在代码里绑定了
            btnDelete.SetBinding(IsEnabledProperty, new Binding(nameof(classes.SelectedUIClass))
            {
                ElementName = nameof(classes),
                Converter = new IsNotNull2BoolConverter()
            });
            btnRename.SetBinding(IsEnabledProperty, new Binding(nameof(classes.SelectedUIClass))
            {
                ElementName = nameof(classes),
                Converter = new IsNotNull2BoolConverter()
            });
            btnAddMatchCondition.SetBinding(IsEnabledProperty, new Binding(nameof(classes.SelectedUIClass))
            {
                ElementName = nameof(classes),
                Converter = new IsNotNull2BoolConverter()
            });
            //但是最后一个无论如何都还是绑定不上
            txtName.SetBinding(TextBox.TextProperty,
                new Binding($"{nameof(classes.SelectedUIClass)}.{nameof(UIClass.Class)}.{nameof(Class.Name)}")
                {
                    ElementName = nameof(classes),
                    Mode = BindingMode.TwoWay
                });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void RemoveMatchConditionButton_Click(object sender, RoutedEventArgs e)
        {
            MatchConditions.Remove((sender as FrameworkElement).Tag as MatchCondition);
            for (int i = 0; i < MatchConditions.Count; i++)
            {
                MatchConditions[i].Index = i;
            }
        }

        public override async Task LoadAsync(Project project)
        {
            await base.LoadAsync(project);
            await classes.LoadAsync(project);
        }

        public async Task SaveClassAsync()
        {
            if(classes.SelectedUIClass!=null)
            {
                await SaveClassAsync(classes.SelectedUIClass.Class);
            }
        }
        public async Task SaveClassAsync(Class c)
        {
            await Task.Run(() =>
            {
                if (c != null)
                {
                    foreach (var m in c.MatchConditions)
                    {
                        if (m.Value == null)
                        {
                            Debug.Assert(false);
                            m.Value = "";
                        }
                    }
                    c.MatchConditions.Clear();
                    c.MatchConditions.AddRange(MatchConditions);
                    ClassUtility.SaveClass(c);
                }
            });
        }

        private async void AddClassInButton_Click(object sender, RoutedEventArgs e)
        {
            await classes.AddAsync();
        }

        private async void DeleteClassButton_Click(object sender, RoutedEventArgs e)
        {
            await classes.DeleteSelectedAsync();
            flyDelete.Hide();
        }

        private void AddMatchConditionButton_Click(object sender, RoutedEventArgs e)
        {
            MatchConditions.Add(new MatchCondition() { Index = MatchConditions.Count });
        }

        private async void SelectedUIClassesChanged(object sender, SelectedClassChangedEventArgs e)
        {
            //加个延时，让UI先反应一下
            await Task.Delay(100);
            Class old = e.OldValue;
            if (old != null && e.NewValue != null)
            {
                await SaveClassAsync(old);
            }
            if (classes.SelectedUIClass == null)
            {
                MatchConditions = null;
            }
            else
            {
                MatchConditions = new ObservableCollection<MatchCondition>
                    (classes.SelectedUIClass.Class.MatchConditions.OrderBy(p => p.Index));
            }
            txtName.Text = classes.SelectedUIClass?.Class.Name;

        }

        private async void Flyout_Closed(object sender, object e)
        {
            classes.SelectedUIClass.Class.Name = txtName.Text;
            await SaveClassAsync(classes.SelectedUIClass.Class);

        }
    }


}
