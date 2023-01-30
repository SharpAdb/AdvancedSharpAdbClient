//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using AdvancedSharpAdbClient.SampleApp.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Navigation;

namespace AdvancedSharpAdbClient.SampleApp
{
    public sealed partial class NewControlsPage : ItemsPageBase
    {
        public NewControlsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.UI.Xaml.Controls.NavigationViewItem menuItem = NavigationRootPage.Current.NavigationView.MenuItems.Cast<Microsoft.UI.Xaml.Controls.NavigationViewItem>().First();
            menuItem.IsSelected = true;
            NavigationRootPage.Current.NavigationView.Header = string.Empty;

            Items = ControlInfoDataSource.Instance.Groups.SelectMany(g => g.Items.Where(i => i.BadgeString != null)).OrderBy(i => i.Title).ToList();
            itemsCVS.Source = FormatData();
        }

        private ObservableCollection<GroupInfoList> FormatData()
        {
            IEnumerable<GroupInfoList> query = from item in this.Items
                                               group item by item.BadgeString into g
                                               orderby g.Key
                                               select new GroupInfoList(g) { Key = g.Key };

            ObservableCollection<GroupInfoList> groupList = new(query);

            //Move Preview samples to the back of the list
            if (groupList.Any())
            {
                GroupInfoList previewGroup = groupList?.ElementAt(1);
                if (previewGroup?.Key.ToString() == "Preview")
                {
                    groupList.RemoveAt(1);
                    groupList.Insert(groupList.Count, previewGroup);
                }
            }

            foreach (GroupInfoList item in groupList)
            {
                switch (item.Key.ToString())
                {
                    case "New":
                        item.Title = "What's New";
                        break;
                    case "Updated":
                        item.Title = "Recently Updated Samples";
                        break;
                    case "Preview":
                        item.Title = "Preview Samples";
                        break;
                }
            }

            return groupList;
        }

        protected override bool GetIsNarrowLayoutState()
        {
            return LayoutVisualStates.CurrentState == NarrowLayout;
        }
    }

    public class GroupInfoList : List<object>
    {
        public GroupInfoList(IEnumerable<object> items) : base(items) { }

        public object Key { get; set; }

        public string Title { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
