namespace Shnexy.Models
{
    public interface ISerializedEvent
    {
        Event Deserialize();
        void Serialize(Event curEvent);
    }
}