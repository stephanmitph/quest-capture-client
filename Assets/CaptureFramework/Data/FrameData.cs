    public class FrameData
    {
        public byte MessageType; // 0 = Begin, 1 = Data, 2 = End
        public int CollectionId;
        public byte[] ImageData;
        public string TrackingJson;

        public FrameData(byte messageType = 0, int collectionId = 0, byte[] imageData = null, string trackingJson = null)
        {
            MessageType = messageType;
            CollectionId = collectionId;
            ImageData = imageData;
            TrackingJson = trackingJson;
        }
    }