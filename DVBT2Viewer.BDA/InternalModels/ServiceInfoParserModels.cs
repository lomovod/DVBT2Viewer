using System.Collections.Generic;

namespace DVBT2Viewer.BDA.InternalModels
{
    #region SDT info
    class SDTChannelModel
    {
        public short TSID { get; set; }
        public short SID { get; set; }
        public bool Scrambled { get; set; }
        public int ChannelType { get; set; }
        public string ChannelName { get; set; }
        public string ProviderName { get; set; }
    }
    #endregion

    #region PAT info
    class PATChannelModel
    {
        public short SID { get; set; }
        public short PMTPid { get; set; }
    }
    #endregion

    #region PMT item info
    class PMTModel
    {
        public short PID { get; set; }
        public byte StreamType { get; set; }
    }
    #endregion

    #region PMT item list
    class PMTChannelModel : PATChannelModel
    {
        private readonly IList<PMTModel> pmtList = new List<PMTModel>();
        public IList<PMTModel> PMTList { get { return pmtList; } }
    }
    #endregion
}
