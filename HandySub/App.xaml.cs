﻿using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Module.Core;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using HandySub.Language;
using HandySub.ViewModels;
using HandySub.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using HandyControl.Themes;

namespace HandySub
{
    public partial class App : PrismApplication
    {
        #region Module
        private readonly string MODULES_PATH = AppDomain.CurrentDomain.BaseDirectory + "modules";
        private ObservableCollection<ModuleModel> moduleCollection = null;

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new DirectoryModuleCatalog() { ModulePath = MODULES_PATH };
        }
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            DirectoryModuleCatalog directoryCatalog = (DirectoryModuleCatalog)moduleCatalog;
            directoryCatalog.Initialize();

            moduleCollection = new ObservableCollection<ModuleModel>();
            TypeFilter typeFilter = new TypeFilter(InterfaceFilter);

            try
            {
                foreach (IModuleCatalogItem item in directoryCatalog.Items)
                {
                    ModuleInfo mi = (ModuleInfo)item;
                    // in .NetFrameWork we dont need to replace
                    Assembly asm = Assembly.LoadFrom(mi.Ref.Replace(@"file:///", ""));

                    foreach (Type t in asm.GetTypes())
                    {
                        Type[] myInterfaces = t.FindInterfaces(typeFilter, typeof(IModuleService).ToString());

                        if (myInterfaces.Length > 0)
                        {
                            IModuleService moduleService = (IModuleService)asm.CreateInstance(t.FullName);

                            ModuleModel module = moduleService.GetModule();

                            moduleCollection.Add(module);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error(ex.Message);
            }
        }

        private bool InterfaceFilter(Type typeObj, object criteriaObj)
        {
            return typeObj.ToString() == criteriaObj.ToString();
        }
        #endregion
        public static string[] WindowsContextMenuArgument = { string.Empty, string.Empty };

        private readonly List<string> wordsToRemove = ". hdtv exm RMT DD5 YTS TURKISH VIDEOFLIX Gisaengchung KOREAN 8CH BluRay Hdcam HDCAM . - XviD AC3 EVO WEBRip FGT MP3 CMRG Pahe 10bit 720p 1080p 480p WEB-DL H264 H265 x264 x265 800MB 900MB HEVC PSA RARBG 6CH 2CH CAMRip Rip AVS RMX HDTV RMTeam mSD SVA MkvCage MeGusta TBS AMZN DDP5.1 DDP5 SHITBOX NITRO WEB DL 1080 720 480 MrMovie BWBP NTG "
           .Split(' ').ToList();

        public App()
        {
            GlobalDataHelper<AppConfig>.Init($"{AppDomain.CurrentDomain.BaseDirectory}AppConfig.json");
            LocalizationManager.Instance.LocalizationProvider = new ResxProvider();
            LocalizationManager.Instance.CurrentCulture = new System.Globalization.CultureInfo(GlobalDataHelper<AppConfig>.Config.UILang);


            //init Appcenter Crash Reporter
            AppCenter.Start("3770b372-60d5-49a1-8340-36a13ae5fb71",
                   typeof(Analytics), typeof(Crashes));
            AppCenter.Start("3770b372-60d5-49a1-8340-36a13ae5fb71",
                               typeof(Analytics), typeof(Crashes));
        }

        public string RemoveJunkString(string stringToClean)
        {
            string cleaned = Regex.Replace(stringToClean, "\\b" + string.Join("\\b|\\b", wordsToRemove) + "\\b", " ");
            cleaned = Regex.Replace(cleaned, @"S[0-9].{1}E[0-9].{1}", ""); // remove SXXEXX ==> X is 0-9
            cleaned = Regex.Replace(cleaned, @"s[0-9].{1}e[0-9].{1}", ""); // remove SXXEXX ==> X is 0-9
            cleaned = Regex.Replace(cleaned, @"(\[[^\]]*\])|(\([^\)]*\))", ""); // remove between () and []
            cleaned = Regex.Replace(cleaned, "[ ]{2,}", " "); // remove space [More than 2 space] and replace with one space

            return cleaned.Trim();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ConfigHelper.Instance.SetLang(GlobalDataHelper<AppConfig>.Config.UILang);

            if (e.Args.Length > 0)
            {
                string NameFromContextMenu = RemoveJunkString(Path.GetFileNameWithoutExtension(e.Args[0]));

                WindowsContextMenuArgument[0] = NameFromContextMenu;
                WindowsContextMenuArgument[1] = e.Args[0].Replace(Path.GetFileName(e.Args[0]), "");
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Container.Resolve<IRegionManager>().RequestNavigate("ContentRegion", "Subscene");
        }

        protected override System.Windows.Window CreateShell()
        {
            MainWindow shell = Container.Resolve<MainWindow>();
            if (moduleCollection != null)
            {
                LeftMainContentViewModel.Instance.DataService.AddRange(moduleCollection);
            }
            if (GlobalDataHelper<AppConfig>.Config.Skin != SkinType.Default)
            {
                UpdateSkin(GlobalDataHelper<AppConfig>.Config.Skin);
            }
            return shell;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MainContent>();
            containerRegistry.RegisterForNavigation<LeftMainContent>();
            containerRegistry.RegisterForNavigation<About>();
            containerRegistry.RegisterForNavigation<Settings>();
            containerRegistry.RegisterForNavigation<Updater>();
            containerRegistry.RegisterForNavigation<PopularSeries>();
            containerRegistry.RegisterForNavigation<Subscene>();
            containerRegistry.RegisterForNavigation<SubsceneDownload>();
            containerRegistry.RegisterForNavigation<GetMovieInfoIMDB>();
        }

        public void UpdateSkin(SkinType skin)
        {
            SharedResourceDictionary.SharedDictionaries.Clear();
            Resources.MergedDictionaries.Add(ResourceHelper.GetSkin(skin));
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
            });
            Current.MainWindow?.OnApplyTemplate();
        }
    }
}
