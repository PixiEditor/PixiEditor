using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace PixiEditor.Models.Layers
{
    [Serializable]
    public class LightLayer : BasicLayer
    {
        private byte[] _layerBytes;

        public byte[] LayerBytes
        {
            get => _layerBytes;
            set
            {
                _layerBytes = value;
                RaisePropertyChanged("LayerBytes");
            }
        }

        public LightLayer(int width, int height)
        {
            LightLayer layer = LayerGenerator.GenerateWithByteArray(width, height);
            LayerBytes = layer.LayerBytes;
            Width = width;
            Height = height;
        }

        public LightLayer(byte[] layerBytes, int height, int width)
        {
            LayerBytes = layerBytes;
            Width = height;
            Height = width;
        }

        public LightLayer()
        {

        }

        public static LightLayer Deserialize(object value)
        {
            return JsonConvert.DeserializeObject<LightLayer>(((JObject)value).ToString());
        }

    }
}
