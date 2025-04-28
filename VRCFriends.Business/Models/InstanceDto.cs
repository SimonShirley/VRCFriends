using System.ComponentModel;
using System.Runtime.CompilerServices;
using VRChat.API.Model;

namespace VRCFriends.Business.Models
{
    public class InstanceDto : INotifyPropertyChanged
    {
        private InstanceRegion? _region;
        private InstanceType? _instanceType;

        public string WorldName { get; set; }
        public InstanceRegion? Region
        {
            get => _region;
            set
            {
                _region = value;
                OnPropertyChanged(nameof(Region));
                OnPropertyChanged(nameof(RegionString));
            }
        }

        public InstanceType? InstanceType
        {
            get => _instanceType;
            set
            {
                _instanceType = value;
                OnPropertyChanged(nameof(InstanceType));
                OnPropertyChanged(nameof(InstanceTypeString));
            }
        }

        public string RegionString
        {
            get
            {
                if (WorldName == "In a Private World")
                    return string.Empty;

                return $"Region: {Region.ToString().ToUpper()}";
            }
        }

        public string InstanceTypeString
        {
            get {
                if (WorldName == "In a Private World")
                    return string.Empty;

                return $"Instance Type: {GetInstanceTypeString(InstanceType)}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string GetInstanceTypeString(InstanceType? instanceType)
        {
            if (instanceType is null)
                return string.Empty;

            switch(instanceType)
            {
                case VRChat.API.Model.InstanceType.Public:
                    return "Public";

                case VRChat.API.Model.InstanceType.Private:
                    return "Private";

                case VRChat.API.Model.InstanceType.Hidden:
                    return "Friends+";

                case VRChat.API.Model.InstanceType.Friends:
                    return "Friends";

                case VRChat.API.Model.InstanceType.Group:
                    return "Group";

                default:
                    return string.Empty;
            };
        }
    }
}
