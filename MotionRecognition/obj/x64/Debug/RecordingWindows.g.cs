﻿#pragma checksum "..\..\..\RecordingWindows.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "672A141FCF302CBC91891D932C65ACF7"
//------------------------------------------------------------------------------
// <auto-generated>
//     這段程式碼是由工具產生的。
//     執行階段版本:4.0.30319.42000
//
//     對這個檔案所做的變更可能會造成錯誤的行為，而且如果重新產生程式碼，
//     變更將會遺失。
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace MotionRecognition {
    
    
    /// <summary>
    /// RecordingWindows
    /// </summary>
    public partial class RecordingWindows : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 7 "..\..\..\RecordingWindows.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image RecodingIcon;
        
        #line default
        #line hidden
        
        
        #line 10 "..\..\..\RecordingWindows.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button RecodingButton;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\..\RecordingWindows.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label LB;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/MotionRecognition;component/recordingwindows.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\RecordingWindows.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 4 "..\..\..\RecordingWindows.xaml"
            ((MotionRecognition.RecordingWindows)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.MainWindow_Closing);
            
            #line default
            #line hidden
            
            #line 4 "..\..\..\RecordingWindows.xaml"
            ((MotionRecognition.RecordingWindows)(target)).Loaded += new System.Windows.RoutedEventHandler(this.MainWindow_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.RecodingIcon = ((System.Windows.Controls.Image)(target));
            return;
            case 3:
            
            #line 9 "..\..\..\RecordingWindows.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.BackButton_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.RecodingButton = ((System.Windows.Controls.Button)(target));
            
            #line 10 "..\..\..\RecordingWindows.xaml"
            this.RecodingButton.Click += new System.Windows.RoutedEventHandler(this.RecodingButton_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.LB = ((System.Windows.Controls.Label)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

