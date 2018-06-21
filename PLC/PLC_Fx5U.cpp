#include "StdAfx.h"
#include "PLC_Fx5U.h"
#define SLEEP_TIME 5
#define WAIT_TIME_OUT 5000
CPLC_Fx5U::CPLC_Fx5U(void)
{
	//m_hMutexSync=NULL;
	m_pEtherPort=NULL;
}

CPLC_Fx5U::CPLC_Fx5U(CEthernetPort* pEthernetPort)
{
	//m_hMutexSync=NULL;
	m_strProtocolHeader="50,00,00,FF,FF,03,00,DataLength,10,00,";
	m_pEtherPort=pEthernetPort;
	InitializeCriticalSection(&m_cs);
	m_mapThread.clear();
}

CPLC_Fx5U::CPLC_Fx5U(CComPort* pComPort)
{
	m_pComPort=pComPort;
}
CPLC_Fx5U::~CPLC_Fx5U(void)
{
	DeleteCriticalSection(&m_cs);
}



//************************************
// Method:    初始化PLC，并打开网口
// FullName:  CPLC_Fx5U::InitPLC
// Access:    public 
// Returns:   bool 成功返回true,失败返回false
//************************************
bool CPLC_Fx5U::InitPLC()
{
	//m_hMutexSync=CreateMutex(NULL,false,"MutexSync");	//线程同步
	bool bRet=false;
	EnterCriticalSection(&m_cs);	//保护状态
	m_nState=0;
	m_mapThread.clear();
	if(m_pEtherPort!=NULL)
	{
		m_pEtherPort->Close();
		Sleep(100);
		if(m_pEtherPort->Open())
		{
				m_nState=1;
				bRet=true;	
		}
	}
	LeaveCriticalSection(&m_cs);
	return bRet;
}



//************************************
// Method:    随机读取一个Bit软元件
// FullName:  CPLC_Fx5U::ReadSingleBit
// Access:    public 
// Returns:   int 成功返回 0 或者 1,失败返回FAILED_PLC
// Parameter: char * BitName, 待读取寄存器的名称，例如"M120"。
//************************************
int CPLC_Fx5U::ReadSingleBit(char* BitName)
{
	int nRet=FAILED_PLC;
	vector<string> vecSplitString;
	char strConvert[200],*strStop;
	BYTE byteRecv[100];
	memset(strConvert,0,sizeof(strConvert));
	ConvertName2Hex(BitName,strConvert);

	CString str=m_strProtocolHeader;

	if(m_pEtherPort!=NULL && m_pEtherPort->IsOpen())
	{
		CString strCmd="01,04,01,00,";
		CString strBitNum="08,00";
		TRACE(m_strProtocolHeader);
		CString strHeader=m_strProtocolHeader;
		strHeader.Replace("DataLength","0C,00");
		CString strSend;

		strSend.Format("%s%s%s%s",strHeader,strCmd,strConvert,strBitNum);
		string strSrc=strSend;
		CSystem::_Instance()->split(strSrc,vecSplitString,",");
		int len=vecSplitString.size();
		BYTE* dataSend=(BYTE*)malloc(len*sizeof(BYTE));

		for(int i=0;i<len;i++)
		{
			TRACE("%s\n",vecSplitString[i].c_str());
			CString str=vecSplitString[i].c_str();
			dataSend[i]=strtol(str,&strStop,16);
			str.ReleaseBuffer();
		}
		EnterCriticalSection(&m_cs);
		//if(WaitForSingleObject(m_hMutexSync,WAIT_TIME_OUT)==WAIT_OBJECT_0)//等待超时5秒钟
		
		if(m_pEtherPort->WriteData(dataSend,len))
		{
			Sleep(SLEEP_TIME);	//等待100ms再去读取返回值
			memset(byteRecv,0,sizeof(byteRecv));
			m_pEtherPort->ReadData(byteRecv,len,100);	//检查读取到的数据
			if(CheckRecvData(byteRecv))//结束代码是0就是成功
			{
				if(byteRecv[11]<0x10)	//说明高位不是1
					nRet=0;
				else
					nRet=1;		
			}
		}
		LeaveCriticalSection(&m_cs);
			//ReleaseMutex(m_hMutexSync);	//释放互斥对象
		
		free(dataSend);	
	}
	return nRet;
}

//************************************
// Method:    读取一个字软元件
// FullName:  CPLC_Fx5U::ReadSingleWord
// Access:    public 
// Returns:   int 成功返回SUCCESS_PLC,失败返回FAILED_PLC
// Parameter: char * WordName, 待读取寄存器的名称，例如"D123","M120"。
//************************************
int CPLC_Fx5U::ReadSingleWord(char* WordName,int nTimeOut)
{
	
	int nRet=FAILED_PLC;
	vector<string> vecSplitString;
	char strConvert[200],*strStop;
	BYTE byteRecv[100];
	memset(strConvert,0,sizeof(strConvert));
	ConvertName2Hex(WordName,strConvert);
	if(m_pEtherPort!=NULL && m_pEtherPort->IsOpen())
	{
		CString strCmd="01,04,00,00,";
		CString strWordNum="01,00";
		CString strHeader=m_strProtocolHeader;
		strHeader.Replace("DataLength","0C,00");
		CString strSend;
		strSend.Format("%s%s%s%s",strHeader,strCmd,strConvert,strWordNum);
		string strSrc=strSend;
		CSystem::_Instance()->split(strSrc,vecSplitString,",");
		int len=vecSplitString.size();
		BYTE* dataSend=(BYTE*)malloc(len*sizeof(BYTE));
		for(int i=0;i<len;i++)
			dataSend[i]=strtol(vecSplitString[i].c_str(),&strStop,16);

		
			//Sleep(1000);
			//TRACE("Reade single world ------ Start\n");
			memset(byteRecv,0,sizeof(byteRecv));
			//TRACE("Ready Read Enter--------->%s",WordName);
			EnterCriticalSection(&m_cs);
			//TRACE("Read Enter------->%s\n",WordName);
			//TRACE("Enter, 线程ID:%X\n",m_cs.OwningThread);
			m_pEtherPort->WriteData(dataSend,len,nTimeOut);
			Sleep(SLEEP_TIME);	//等待再去读取返回值
			m_pEtherPort->ReadData(byteRecv,len,100);	//检查读取到的数据
			//TRACE("Ready Read Leave------>%s--",WordName);
			LeaveCriticalSection(&m_cs);
			//TRACE("Read Leave %s\n",WordName);
			if(CheckRecvData(byteRecv))//结束代码是0就是成功
			{
				nRet=byteRecv[11]+(byteRecv[12]<<8);		
			}
		free(dataSend);	
	}
	
	//TRACE("Reade single world ------ end\n");
	return nRet;
}

//************************************
// Method:    同时读多个字的Bit
// FullName:  CPLC_Fx5U::ReadMutiBit
// Access:    public 
// Returns:   成功-SUCCESS_PLC，失败-FAILED_PLC。
// Parameter: char * BitName, 待读取寄存器的名称，例如"M120"。
// Parameter: bool* bArray, 用来存放读取后的结果的bool型数组。
// Parameter: int nNum, Bit个数，以字为单位。
//************************************
int CPLC_Fx5U::ReadMutiBit(char* BitName,bool* bArray,int nNum)	//读取nNum个字的位软元件
{
	int nRet=FAILED_PLC;
	vector<string> vecSplitString;
	char strConvert[200],*strStop;
	BYTE byteRecv[100];
	memset(strConvert,0,sizeof(strConvert));
	ConvertName2Hex(BitName,strConvert);
	if(m_pEtherPort!=NULL && m_pEtherPort->IsOpen())
	{
		CString strCmd="01,04,00,00,";
		CString strWordNum;
		strWordNum.Format("%x,%x",nNum & 0xFF,(nNum>>16)&0xFF);
		CString strHeader=m_strProtocolHeader;
		strHeader.Replace("DataLength","0C,00");
		CString strSend;
		strSend.Format("%s%s%s%s",strHeader,strCmd,strConvert,strWordNum);
		string strSrc=strSend;
		CSystem::_Instance()->split(strSrc,vecSplitString,",");
		int len=vecSplitString.size();
		BYTE* dataSend=(BYTE*)malloc(len*sizeof(BYTE));
		for(int i=0;i<len;i++) 
			dataSend[i]=strtol(vecSplitString[i].c_str(),&strStop,16);
		//if(WaitForSingleObject(m_hMutexSync,WAIT_TIME_OUT)==WAIT_OBJECT_0)//等待超时5秒钟
		EnterCriticalSection(&m_cs);
		{
			if(m_pEtherPort->WriteData(dataSend,len))
			{
				Sleep(SLEEP_TIME);	//等待100ms再去读取返回值
				memset(byteRecv,0,sizeof(byteRecv));
				m_pEtherPort->ReadData(byteRecv,len,sizeof(byteRecv));	//检查读取到的数据
				if(CheckRecvData(byteRecv))//结束代码是0就是成功
				{
					for(int i=0;i<nNum*2;i++)	//nNum个字元件
						for(int j=0;j<8;j++)
							bArray[i*8+j]=(byteRecv[i+11]>>j)&0x01;
					nRet=SUCCESS_PLC;
				}
			}
			LeaveCriticalSection(&m_cs);
			//ReleaseMutex(m_hMutexSync);
		}
		free(dataSend);	
	}
	return nRet;
}
int CPLC_Fx5U::ReadMutiWord(char* WordName,int* iArray, int nNum)	//读取nNum个字的字软元件
{
	int nRet=FAILED_PLC;
	vector<string> vecSplitString;
	char strConvert[200],*strStop;
	BYTE byteRecv[100];
	memset(strConvert,0,sizeof(strConvert));
	ConvertName2Hex(WordName,strConvert);
	if(m_pEtherPort!=NULL /*&& m_pEtherPort->IsOpen()*/)
	{
		CString strCmd="01,04,00,00,";	//读命令
		CString strWordNum;
		strWordNum.Format("%x,%x",nNum & 0xFF,(nNum>>16)&0xFF);
		CString strHeader=m_strProtocolHeader;
		strHeader.Replace("DataLength","0C,00");
		CString strSend;
		strSend.Format("%s%s%s%s",strHeader,strCmd,strConvert,strWordNum);
		string strSrc=strSend;
		CSystem::_Instance()->split(strSrc,vecSplitString,",");
		int len=vecSplitString.size();
		BYTE* dataSend=(BYTE*)malloc(len*sizeof(BYTE));
		for(int i=0;i<len;i++)
			dataSend[i]=strtol(vecSplitString[i].c_str(),&strStop,16);
		EnterCriticalSection(&m_cs);
		//if(WaitForSingleObject(m_hMutexSync,WAIT_TIME_OUT)==WAIT_OBJECT_0)//等待超时5秒钟
		{
			if(m_pEtherPort->WriteData(dataSend,len))
			{
				Sleep(SLEEP_TIME);	//等待100ms再去读取返回值
				memset(byteRecv,0,sizeof(byteRecv));
				m_pEtherPort->ReadData(byteRecv,len,100);	//检查读取到的数据
				if(CheckRecvData(byteRecv))//结束代码是0就是成功
				{
					for(int i=0;i<nNum;i++)	//nNum个字元件
						iArray[i]=(int)byteRecv[i*2+11]+(int)(byteRecv[i*2+12]<<16);//L,H
					nRet=SUCCESS_PLC;
				}
			}
			LeaveCriticalSection(&m_cs);
			//ReleaseMutex(m_hMutexSync);
		}
		free(dataSend);			
	}

	return nRet;
}


//************************************
// Method:    随机写一个Bit软元件
// FullName:  CPLC_Fx5U::WriteSingleBit
// Access:    public 
// Returns:   成功返回true ,失败返回false
// Parameter: char * BitName, 待写入寄存器的名称，例如"M120"。
// Parameter: bool bValue, 待写入寄存器的bool值。
//************************************
bool CPLC_Fx5U::WriteSingleBit(char* BitName,bool bValue)
{
	bool bRet=false;
	if(m_pEtherPort==NULL || !m_pEtherPort->IsOpen())
		bRet = false;
	char* strCmd="01,14,00,00,";	//写命令
	vector<string> vecSplitString;
	char strConvert[200],*strStop;
	BYTE byteRecv[100];
	memset(strConvert,0,sizeof(strConvert));
	ConvertName2Hex(BitName,strConvert);	//元件代码以及其实地址
	char* strNum="08,00,";			//写入的软元件个数(Bit为单位)
	CString strHeader=m_strProtocolHeader;
	strHeader.Replace("DataLength","10,00");
	CString strData(""),strSend("");
	bool bArray[16];	//读取一个字的长度
	int iValue=0;
	BYTE* dataSend;
	//if(WaitForSingleObject(m_hMutexSync,WAIT_TIME_OUT)==WAIT_OBJECT_0)//等待超时5秒钟
	EnterCriticalSection(&m_cs);
	{
		if(FAILED_PLC==ReadMutiBit(BitName,bArray,1))
			bRet= false;

		for(int i=0;i<16;i++)
		{
			if(i==0)
				iValue+=Bool2Int(bValue)<<i	;	//将最低位改写成设置的值
			else
				iValue+=Bool2Int(bArray[i])<<i;
		}
		for(int i=0;i<8;i+=2)
		{
			CString strTemp;
			strTemp.Format("%d%d,",((iValue>>i)&0x1),(iValue>>(i+1)&0x1));
			strData+=strTemp;
		}
		strSend.Format("%s%s%s%s%s",strHeader,strCmd,strConvert,strNum,strData);
		string strSrc=strSend;
		CSystem::_Instance()->split(strSrc,vecSplitString,",");
		int len=vecSplitString.size();
		dataSend=(BYTE*)malloc(len*sizeof(BYTE));
		for(int i=0;i<len;i++)
			dataSend[i]=strtol(vecSplitString[i].c_str(),&strStop,16);
		if(m_pEtherPort!=NULL)
		{
			if(m_pEtherPort->WriteData(dataSend,len))	//将生成的发送出去
			{
				Sleep(SLEEP_TIME);
				memset(byteRecv,0,sizeof(byteRecv));
				m_pEtherPort->ReadData(byteRecv,sizeof(byteRecv),1000);	//检查读取到的数据
				if(CheckRecvData(byteRecv))//结束代码是0就是成功
					bRet= true;
			}
		}
		LeaveCriticalSection(&m_cs);
		//ReleaseMutex(m_hMutexSync);
	}
	free(dataSend);
	return bRet;
}

//************************************
// Method:    随机写一个Word软元件
// FullName:  CPLC_Fx5U::WriteSingleWord
// Access:    public 
// Returns:   成功返回true ,失败返回false
// Parameter: char * WordName, 待写入寄存器的名称，例如"D120"。
// Parameter: int Value, 待写入寄存器的值。
//************************************
bool CPLC_Fx5U::WriteSingleWord(char* WordName,int iValue)
{
	
	bool bRet=false;
	char* strCmd="01,14,00,00,";	//写命令
	vector<string> vecSplitString;
	char strConvert[200],*strStop;
	BYTE byteRecv[100];
	memset(strConvert,0,sizeof(strConvert));
	ConvertName2Hex(WordName,strConvert);	//元件代码以及其实地址
	char* strNum="01,00,";			//写入的软元件个数(Bit为单位)


	//替换发送数据的长度
	CString strHeader=m_strProtocolHeader;
	strHeader.Replace("DataLength","0E,00");

	CString strData(""),strSend("");
	strData.Format("%X,%X,",iValue&0xFF,(iValue>>8) & 0xFF);

	strSend.Format("%s%s%s%s%s",strHeader,strCmd,strConvert,strNum,strData);
	string strSrc=strSend;
	CSystem::_Instance()->split(strSrc,vecSplitString,",");
	int len=vecSplitString.size();
	BYTE* dataSend=(BYTE*)malloc(len*sizeof(BYTE));
	for(int i=0;i<len;i++)
		dataSend[i]=strtol(vecSplitString[i].c_str(),&strStop,16);
	if(m_pEtherPort!=NULL && m_pEtherPort->IsOpen())
	{
		memset(byteRecv,0,sizeof(byteRecv));
		//TRACE("Ready Write Enter----->%s--",WordName);

		EnterCriticalSection(&m_cs);
		//TRACE("Write Enter----->%s\n",WordName);
		m_pEtherPort->WriteData(dataSend,len);	//将生成的发送出去
		Sleep(SLEEP_TIME);
		m_pEtherPort->ReadData(byteRecv,sizeof(byteRecv),1000);	//检查读取到的数据
		//TRACE("Ready Write Leave----->%s--",WordName);
		LeaveCriticalSection(&m_cs);
		//TRACE("Write Leave %s\n",WordName);
		if(CheckRecvData(byteRecv))//结束代码是0就是成功
			bRet = true;
	}
	free(dataSend);
	return bRet;
}


//************************************
// Method:    同时写多个字的Bit
// FullName:  CPLC_Fx5U::WriteMutiBit
// Access:    public 
// Returns:   成功-true，失败-false。
// Parameter: char * BitName, 待读取寄存器的名称，例如"M120"。
// Parameter: bool* bArray, 用来存放读取后的结果的bool型数组。
// Parameter: int nNum, Bit个数，以字为单位。
//************************************
bool CPLC_Fx5U::WriteMutiBit(char* BitName,bool* bArray,int nNum)
{
	EnterCriticalSection(&m_cs);
	bool bRet=false;
	char* strCmd="01,14,00,00,";	//写命令
	vector<string> vecSplitString;
	char strConvert[200],*strStop;
	BYTE byteRecv[100];
	memset(strConvert,0,sizeof(strConvert));
	ConvertName2Hex(BitName,strConvert);	//元件代码以及其实地址
	CString strNum;			//写入的软元件个数(Bit为单位)
	strNum.Format("%X,%X,",nNum & 0xFF,(nNum>>8)&0xFF);

	//替换发送数据的长度
	int dataLen=12+nNum*2;
	CString strLen;
	strLen.Format("%X,%X",dataLen&0xFF,(dataLen>>8)&0xFF);
	CString strHeader=m_strProtocolHeader;
	strHeader.Replace("DataLength",strLen);

	CString strData(""),strSend(""),strTemp("");
	for(int i=0;i<2*nNum;i++)
	{
		int iValue=0;
		for(int j=0;j<8;j++)
		{
			iValue |=(Bool2Int(bArray[j])<<j);
		}
		strTemp.Format("%X,%X",(iValue>>4) & 0xF,iValue & 0xF);//高4位，低四位
		if(i<2*nNum-1)
			strTemp+=",";
		strData+=strTemp;
	}

	strSend.Format("%s%s%s%s%s",strHeader,strCmd,strConvert,strNum,strData);
	string strSrc=strSend;
	CSystem::_Instance()->split(strSrc,vecSplitString,",");
	int len=vecSplitString.size();
	BYTE* dataSend=(BYTE*)malloc(len*sizeof(BYTE));
	for(int i=0;i<len;i++)
		dataSend[i]=strtol(vecSplitString[i].c_str(),&strStop,16);
	if(m_pEtherPort!=NULL && m_pEtherPort->IsOpen())
	{
		//if(WaitForSingleObject(m_hMutexSync,WAIT_TIME_OUT)==WAIT_OBJECT_0)//等待超时5秒钟
		
		{
			if(m_pEtherPort->WriteData(dataSend,len))	//将生成的发送出去
			{
				Sleep(SLEEP_TIME);
				memset(byteRecv,0,sizeof(byteRecv));
				m_pEtherPort->ReadData(byteRecv,sizeof(byteRecv),1000);	//检查读取到的数据
				if(CheckRecvData(byteRecv))//结束代码是0就是成功
					bRet = true;
			}
		}
		//ReleaseMutex(m_hMutexSync);
	}
	free(dataSend);
	LeaveCriticalSection(&m_cs);
	return bRet;
}

bool CPLC_Fx5U::WriteMutiWord(char* WordName,int* iArray,int nNum)
{
	EnterCriticalSection(&m_cs);
	bool bRet=false;
	char* strCmd="01,14,00,00,";	//写命令
	vector<string> vecSplitString;
	char strConvert[200],*strStop;
	BYTE byteRecv[100];
	memset(strConvert,0,sizeof(strConvert));
	ConvertName2Hex(WordName,strConvert);	//元件代码以及其实地址
	CString strNum;			//写入的软元件个数(word为单位)
	strNum.Format("%X,%X,",nNum & 0xFF,(nNum>>8)&0xFF);
	//替换发送数据的长度
	int dataLen=12+nNum*2;
	CString strLen;
	strLen.Format("%X,%X",dataLen&0xFF,(dataLen>>8)&0xFF);
	CString strHeader=m_strProtocolHeader;
	strHeader.Replace("DataLength",strLen);

	CString strData(""),strSend(""),strTemp("");
	for(int i=0;i<nNum;i++)
	{
		for(int j=0;j<16;j++)
		{
			strTemp.Format("%X,%X",iArray[i] & 0xFF,(iArray[i]>>8) & 0xFF);//低8位，高8位
			if(i<nNum-1)
				strTemp+=",";
		}
		strData+=strTemp;
	}
	strSend.Format("%s%s%s%s%s",strHeader,strCmd,strConvert,strNum,strData);
	string strSrc=strSend;
	CSystem::_Instance()->split(strSrc,vecSplitString,",");
	int len=vecSplitString.size();
	BYTE* dataSend=(BYTE*)malloc(len*sizeof(BYTE));
	for(int i=0;i<len;i++)
		dataSend[i]=strtol(vecSplitString[i].c_str(),&strStop,16);
	if(m_pEtherPort!=NULL /*&& m_pEtherPort->IsOpen()*/)
	{
		TRACE("WaitForSingle Object for 5s\n");
		//if(WaitForSingleObject(m_hMutexSync,WAIT_TIME_OUT)==WAIT_OBJECT_0)//等待超时5秒钟
		{
			if(m_pEtherPort->WriteData(dataSend,len))	//将生成的发送出去
			{
				Sleep(SLEEP_TIME);
				memset(byteRecv,0,sizeof(byteRecv));
				m_pEtherPort->ReadData(byteRecv,sizeof(byteRecv),1000);	//检查读取到的数据
				if(CheckRecvData(byteRecv))//结束代码是0就是成功
					bRet=true;
			}
			//ReleaseMutex(m_hMutexSync);
		}
	}
	TRACE("WaitForSingle Object for 5s___TimeOut\n");
	free(dataSend);
	LeaveCriticalSection(&m_cs);
	return bRet;
}

bool CPLC_Fx5U::ConvertName2Hex(const char* strNameSrc, char* strDes)
{
	CString str=strNameSrc;
	int nValue=atoi(str.Right(str.GetLength()-1));
	if(str[0]=='M' || str[0]=='m')
	{
		sprintf_s(strDes,100,"%X,%X,%X,90,",nValue&0XFF,(nValue>>8)&0xFF,(nValue>>16)&0xFF);
	}
	else if(str[0]=='D' || str[0]=='d')
	{
		sprintf_s(strDes,100,"%X,%X,%X,A8,",nValue&0XFF,(nValue>>8)&0xFF,(nValue>>16)&0xFF);
	}
	else if(str[0]=='W' || str[0]=='w')
	{
		sprintf_s(strDes,100,"%X,%X,%X,B4,",nValue&0XFF,(nValue>>8) &0xFF,(nValue>>16)&0xFF);
	}
	else if(str[0]=='T' || str[0]=='t')
	{
		sprintf_s(strDes,100,"%X,%X,%X,C2,",nValue&0XFF,(nValue>>8) &0xFF,(nValue>>16)&0xFF);
	}
	else
	{
		return false;
	}
	return true;
}
void CPLC_Fx5U::InitLog(LPVOID pVoid)
{
	string strThreadName((char*)pVoid);
	EnterCriticalSection(&m_cs);
		m_mapThread.insert(make_pair(m_cs.OwningThread,strThreadName));
	LeaveCriticalSection(&m_cs);
}

bool CPLC_Fx5U::CheckRecvData(const BYTE* dataArray)
{
	return (dataArray[0]==0xD0 &&
			dataArray[1]==0x00 &&
			dataArray[2]==0x00 &&
			dataArray[3]==0xFF &&
			dataArray[4]==0xFF &&
			dataArray[5]==0x03 &&
			dataArray[6]==0x00 &&
			dataArray[9]==0x00 &&
			dataArray[10]==0x00);	
}