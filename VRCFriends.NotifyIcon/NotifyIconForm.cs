using System;
using System.Drawing;
using System.Windows.Forms;
using VRCFriends.Business.Interfaces;
using VRCFriends.Business.Interfaces.Friends;

namespace VRCFriends.NotifyIcon
{
    public partial class NotifyIconForm : Form, INotifyIconForm, IDisposable
    {
        private readonly ContextMenuStrip _menu;
        private readonly ToolStripMenuItem _friendsMenuItem;
        private readonly ToolStripMenuItem _exitMenuItem;

        private readonly IStateMediator _stateMediator;
        private readonly IFriendsModel _friendsModel;

        public NotifyIconForm(IStateMediator stateMediator, IFriendsModel friendsModel)
        {
            _stateMediator = stateMediator;
            _friendsModel = friendsModel;

            InitializeComponent();

            _friendsMenuItem = new ToolStripMenuItem { Text = "Show Friends" };
            _exitMenuItem = new ToolStripMenuItem { Text = "E&xit" };

            _menu = new ContextMenuStrip();
            _menu.Items.AddRange(new[] { _friendsMenuItem, _exitMenuItem });

            notifyIcon.ContextMenuStrip = _menu;
            notifyIcon.Icon = new Icon("Assets\\vrchat.ico");
            notifyIcon.DoubleClick += ShowFriendsMenuItem_Click;

            _friendsMenuItem.Click += ShowFriendsMenuItem_Click;
            _exitMenuItem.Click += ExitMenuItem_Click;

            _stateMediator.FriendsListUpdated += StateMediator_FriendsListUpdated;
        }

        private void StateMediator_FriendsListUpdated()
        {
            var onlineFriends = _friendsModel.GetOnlineFriendsCount();

            var friendWord = onlineFriends == 1 ? "friend" : "friends";
            notifyIcon.Text = $"Currently {onlineFriends} {friendWord} online";
        }

        private void ShowFriendsMenuItem_Click(object sender, EventArgs e)
        {
            _stateMediator.OnShowFriendsContextMenuItemClicked();
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            _stateMediator.OnAppExited();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            _friendsMenuItem.Click -= ShowFriendsMenuItem_Click;
            _exitMenuItem.Click -= ExitMenuItem_Click;

            _stateMediator.FriendsListUpdated -= StateMediator_FriendsListUpdated;

            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }
    }
}
