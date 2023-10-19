#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED
using System;
using System.Net;
using System.Collections.Generic;

namespace IPTech.DebugConsoleService.TelnetConsole
{
	public enum EClientStatus
	{
		Guest = 0,
		Authenticating = 1,
		LoggedIn = 2
	}

	public class TelnetClient
	{
		private uint id;
		private IPEndPoint remoteAddr;
		private DateTime connectedAt;
		private EClientStatus status;
		private string receivedData;

		private List<string> messageHistory;
		private int historyIndex;

		public TelnetClient(uint clientId, IPEndPoint remoteAddress)
		{
			this.id = clientId;
			this.remoteAddr = remoteAddress;
			this.connectedAt = DateTime.Now;
			this.status = EClientStatus.Guest;
			this.receivedData = string.Empty;

			this.messageHistory = new List<string>();
		}

		public uint getClientID()
		{
			return id;
		}

		public IPEndPoint getRemoteAddress()
		{
			return remoteAddr;
		}

		public DateTime getConnectionTime()
		{
			return connectedAt;
		}

		public EClientStatus getCurrentStatus()
		{
			return status;
		}

		public string getReceivedData()
		{
			return receivedData;
		}

		public void setStatus(EClientStatus newStatus)
		{
			this.status = newStatus;
		}

		public void setReceivedData(string newReceivedData)
		{
			this.receivedData = newReceivedData;
		}

		public void appendReceivedData(string dataToAppend)
		{
			this.receivedData += dataToAppend;
		}

		public void removeLastCharacterReceived()
		{
			receivedData = receivedData.Substring(0, receivedData.Length - 1);
		}

		public void resetReceivedData()
		{
			receivedData = string.Empty;
		}

		public void pushReceivedDataToHistory() {
			this.messageHistory.Insert(0, this.receivedData);
		}

		public void resetReceivedDataHistoryIterator() {
			this.historyIndex = 0;
		}

		public string getNextReceivedDataHistory() {
			if(this.messageHistory.Count == 0) {
				return null;
			}

			if(this.historyIndex < this.messageHistory.Count) {
				return this.messageHistory[this.historyIndex++];
			}
			return this.messageHistory[this.messageHistory.Count - 1];
		}

		public string getPreviousReceiveDataHistory() {
			if(this.messageHistory.Count == 0) {
				return null;
			}

			if(this.historyIndex > 0) {
				return this.messageHistory[this.historyIndex--];
			}
			return this.messageHistory[0];
		}

		public override string ToString()
		{
			string ip = string.Format("{0}:{1}", remoteAddr.Address.ToString(), remoteAddr.Port);
			string res = string.Format("Client #{0} (From: {1}, Status: {2}, Connection time: {3})", id, ip, status, connectedAt);
			return res;
		}
	}
}
#endif