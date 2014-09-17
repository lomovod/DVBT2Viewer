using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using DVBT2Viewer.BDA.InternalModels;

namespace DVBT2Viewer.BDA.Graph
{
    class ServiceInfoParser : IDisposable
    {
        private readonly IDvbSiParser parser = (IDvbSiParser)new DvbSiParser();

        internal ServiceInfoParser(IMpeg2Data mpeg2Data)
        {
            var hr = parser.Initialize(mpeg2Data);
            DsError.ThrowExceptionForHR(hr);
        }

        #region SDT
        public List<SDTChannelModel> GetSDT()
        {
            var channels = new List<SDTChannelModel>();

            IDVB_SDT sdt;
  
            if (parser.GetSDT(0x42, null, out sdt) != 0 || sdt == null)
                return null;

            try
            {
                int sdtRecordsCount;
                if (sdt.GetCountOfRecords(out sdtRecordsCount) != 0)
                    return null;
                short tsid;
                if (sdt.GetTransportStreamId(out tsid) != 0)
                    return null;

                for (int i = 0; i < sdtRecordsCount; i++)
                {
                    short sid; // service id (SID)
                    bool freeCAmode;
                    byte chType = 0;
                    string chName = string.Empty;
                    string provName = string.Empty;
                    if (sdt.GetRecordServiceId(i, out sid) != 0)
                        continue;
                    if (sdt.GetRecordFreeCAMode(i, out freeCAmode) != 0)
                        continue;
                    IGenericDescriptor genDescriptor2;
                    if (sdt.GetRecordDescriptorByTag(i, 0x48, null, out genDescriptor2) == 0 && genDescriptor2 != null)
                    {
                        try
                        {
                            var servDescriptor = genDescriptor2 as IDvbServiceDescriptor;
                            if (servDescriptor != null)
                            {
                                if (servDescriptor.GetServiceType(out chType) != 0)
                                    continue;
                                if (servDescriptor.GetServiceNameEmphasized(out chName) != 0)
                                    continue;
                                if (servDescriptor.GetServiceProviderNameW(out provName) != 0)
                                    continue;
                            }
                        }
                        finally
                        {
                            Marshal.ReleaseComObject(genDescriptor2);        
                        }
                    }

                    channels.Add(new SDTChannelModel
                    {
                        SID  = sid,
                        TSID = tsid,
                        Scrambled = freeCAmode, 
                        ChannelName = chName,
                        ChannelType = chType,
                        ProviderName = provName
                    });
                }
            }
            finally
            {
                Marshal.ReleaseComObject(sdt);
            }
            return
                channels;
        }
        #endregion

        #region PAT
        public List<PATChannelModel> GetPAT()
        {
            var channels = new List<PATChannelModel>();

            IPAT pat;
            if (parser.GetPAT(out pat) != 0 || pat == null)
                return null;

            try
            {
                int patRecordCount;
                pat.GetCountOfRecords(out patRecordCount);
                for (int i = 0; i < patRecordCount; i++)
                {
                    short sid;
                    if (pat.GetRecordProgramNumber(i, out sid) != 0)
                        continue;
                    short pmtPid;
                    if (pat.FindRecordProgramMapPid(sid, out pmtPid) != 0 || pmtPid <= 0)
                        continue;
                    channels.Add(new PATChannelModel {SID = sid, PMTPid = pmtPid});
                }
                return
                    channels;
            }
            finally
            {
                Marshal.ReleaseComObject(pat);
            }

        }
        #endregion

        #region PMT
        public List<PMTModel> GetPMT(PATChannelModel aChannel)
        {
            var pidlist = new List<PMTModel>();

            IPMT pmt;
            if (parser.GetPMT(aChannel.PMTPid, aChannel.SID, out pmt) != 0 || pmt == null)
                return null;

            try
            {
                short pmtRecordsCount;
                if (pmt.GetCountOfRecords(out pmtRecordsCount) != 0)
                    return null;
                for (int j = 0; j < pmtRecordsCount; j++)
                {
                    byte streamType;
                    short pid;
                    pmt.GetRecordStreamType(j, out streamType);
                    pmt.GetRecordElementaryPid(j, out pid);
                    pidlist.Add(new PMTModel {PID = pid, StreamType = streamType});
                }
                return pidlist;
            }
            finally
            {
                Marshal.ReleaseComObject(pmt);
            }
        }

        public PMTChannelModel GetPMTandPAT(PATChannelModel aChannel)
        {
            var channel = new PMTChannelModel {SID = aChannel.SID, PMTPid = aChannel.PMTPid};
            var pmtList = GetPMT(aChannel);
            if (pmtList == null)
                return null;

            foreach (var pmt in pmtList)
                channel.PMTList.Add(pmt);
            return
                channel;
        }
        #endregion

        public void Dispose()
        {
            Marshal.ReleaseComObject(parser);
        }
    }

}
