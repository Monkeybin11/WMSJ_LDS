#pragma once
#include "plc.h"
class CPLC_Fx5U :
	public CPLC
{
public:
	CPLC_Fx5U(void);
	CPLC_Fx5U(CEthernetPort* pEthernetPort);
	CPLC_Fx5U(CComPort* pComPort);
	bool InitPLC();
	void InitLog(LPVOID);
	bool WriteSingleBit(char* BitName,bool bValue);
	bool WriteSingleWord(char* WordName,int iValue);
	bool WriteMutiBit(char* BitName,bool* bArray, int nNum);
	bool WriteMutiWord(char* WordName,int* iArray,int nNum);

	int ReadSingleBit(char* BitName);
	int ReadSingleWord(char* WordName,int nTimeOut=0);
	int ReadMutiBit(char* BitName,bool* bArray,int nNum);
	int ReadMutiWord(char* WordName,int* iArray,int nNum);
	~CPLC_Fx5U(void);
private:
	bool ConvertName2Hex(const char* strNameSrc,char* strDes);
	inline int Bool2Int(bool bValue){return bValue? 1:0;};
	bool CheckRecvData(const BYTE* dataArray);
	HWND m_hWnd;
	CString m_strProtocolHeader;
	map<HANDLE,string> m_mapThread;
	//HANDLE m_hMutexSync;
	CRITICAL_SECTION m_cs;
};

