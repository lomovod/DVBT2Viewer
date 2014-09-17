using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using DVBT2Viewer.BDA.Interfaces;
using DVBT2Viewer.BDA.Models;

namespace DVBT2Viewer.UI.Helpers
{
    #region DigitalMultiplex Load/Save helper
    class DigitalMultiplexLoadSave
    {
        private DigitalMultiplexLoadSave() { }

        public static void SaveMultiplexToXml(DigitalMultiplex multiplex, string fileName)
        {
            try
            {
                var serializer = new XmlSerializer(typeof (StoreDigitalMultiplex));
                using (var textWriter = new StreamWriter(fileName))
                {
                    serializer.Serialize(textWriter, StoreDigitalMultiplex.FromDigitalMultiplex(multiplex));
                }
            }
            catch
            {
                // Just stub
            }
        }

        public static DigitalMultiplex LoadMultiplexFromXml(string fileName)
        {
            try
            {
                var serializer = new XmlSerializer(typeof (StoreDigitalMultiplex));
                using (var stream = new StreamReader(fileName))
                {
                    using (var reader = XmlReader.Create(stream))
                    {
                        return ((StoreDigitalMultiplex) serializer.Deserialize(reader)).ToDigitalMultiplex();
                    }
                }
            }
            catch
            {
                // Just stub
                return null;
            }
        }
    }
    #endregion

    #region DigitalMultiplex serializable mapper
    [XmlRoot(ElementName = "DigitalMultiplex")]
    public class StoreDigitalMultiplex
    {
        public int Frequency { get; set; } // Frequency in kHz

        public int Bandwidth { get; set; } // Bandwidth in MHz

        [XmlArrayItem(ElementName = "DigitalChannel")]
        public List<StoreDigitalChannel> Channels { get; set; } // Channels

        public static StoreDigitalMultiplex FromDigitalMultiplex(DigitalMultiplex multiplex)
        {
            return new StoreDigitalMultiplex
            {
                Frequency = multiplex.Frequency,
                Bandwidth = multiplex.Bandwidth,
                Channels = new List<StoreDigitalChannel>(multiplex.Channels.Select(StoreDigitalChannel.FromDigitalChannel))
            };
        }

        public DigitalMultiplex ToDigitalMultiplex()
        {
            var resultMultiplex = new DigitalMultiplex
            {
                Frequency = Frequency,
                Bandwidth = Bandwidth,
            };
            foreach (var channel in Channels)
                resultMultiplex.Channels.Add(channel.ToDigitalChannel(resultMultiplex));

            return resultMultiplex;
        }
    }
    #endregion

    #region DigitalChannel serializable mapper
    public class StoreDigitalChannel
    {
        public string ChannelName { get; set; } // Channel name
        public string ProviderName { get; set; } // Provider name
        public short SID { get; set; } // SID
        public short TSID { get; set; } // TSID
        public short PMTPID { get; set; } // PMT PID
        public bool Scrambled { get; set; } // CA sign
        public int StreamId { get; set; } // Stream (PLP) ID
        public DigitalChannelTypes ChannelType { get; set; } // TV / Radio / Other
        public List<DigitalStreamType> StreamTypes { get; set; }

        public static StoreDigitalChannel FromDigitalChannel(DigitalChannel channel)
        {
            return new StoreDigitalChannel
            {
                ChannelName = channel.ChannelName,
                ProviderName = channel.ProviderName,
                SID = channel.SID,
                TSID = channel.TSID,
                PMTPID = channel.PMTPID,
                Scrambled = channel.Scrambled,
                StreamId = channel.StreamId,
                ChannelType = channel.ChannelType,
                StreamTypes = new List<DigitalStreamType>(channel.Streams.Types)
            };
        }

        public DigitalChannel ToDigitalChannel(DigitalMultiplex multiplex)
        {
            var resultChannel = multiplex.CreateChannel();

            resultChannel.ChannelName = ChannelName;
            resultChannel.ProviderName = ProviderName;
            resultChannel.SID = SID;
            resultChannel.TSID = TSID;
            resultChannel.PMTPID = PMTPID;
            resultChannel.Scrambled = Scrambled;
            resultChannel.StreamId = StreamId;
            resultChannel.ChannelType = ChannelType;

            foreach (var streamType in StreamTypes)
                resultChannel.Streams.Types.Add(streamType);

            return resultChannel;
        }
    }
    #endregion
}
