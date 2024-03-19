using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CheckListMaker.Messengers;

/// <summary> アイテム保存の設定変更Messenger </summary>
internal class SaveSettingsChangedMessage(string value)
        : ValueChangedMessage<string>(value)
{
}
