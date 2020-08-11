#include"V0.5.h"
volatile uint8_t zigbeeReceive[ZIGBEE_MESSAGE_LENTH];	//实时记录收到的信息
volatile uint8_t zigbeeMessage[ZIGBEE_MESSAGE_LENTH];//经过整理顺序后得到的信息
volatile int message_index = 0;
volatile int message_head = -1;
uint8_t zigbeeBuffer[1];

UART_HandleTypeDef* zigbee_huart;


volatile struct BasicInfo Game;//储存比赛状态、时间、泄洪口信息
volatile struct CarInfo Car[2];//储存车辆信息
volatile struct PassengerInfo Passenger;//储存人员的信息、位置和送达位置
volatile struct PackageInfo Package[6];//储存防汛物资的信息
volatile struct StopInfo Stop[2];//储存泄洪口位置信息
volatile struct Position Obstacle;//储存虚拟障碍信息
/***********************接口****************************/
void zigbee_Init(UART_HandleTypeDef *huart)
{
	zigbee_huart = huart;
	HAL_UART_Receive_IT(zigbee_huart, zigbeeBuffer, 1);
}
void zigbeeMessageRecord(void)
{
	zigbeeMessage[message_index] = zigbeeBuffer[0];
	message_index = receiveIndexAdd(message_index, 1);    //一个简单的索引数增加函数

	if (zigbeeMessage[receiveIndexMinus(message_index, 2)] == 0x0D
		&& zigbeeMessage[receiveIndexMinus(message_index, 1)] == 0x0A)//一串信息的结尾
	{
		if (receiveIndexMinus(message_index, message_head) == 0)
		{

			int index = message_head;
			for (int i = 0; i < 37; i++)
			{
				zigbeeReceive[i] = zigbeeMessage[index];
				index = receiveIndexAdd(index, 1);
			}
			DecodeAll();
		}
		else
		{
			message_head = message_index;
		}
	}
	HAL_UART_Receive_IT(zigbee_huart, zigbeeBuffer, 1);
}
enum GameStateEnum getGameState(void)
{
	uint8_t state = Game.GameState;
	if (state == 0)
	{
		return GameNotStart;
	}
	else if (state == 1)
	{
		return GameGoing;
	}
	else if (state == 2)
	{
		return GamePause;
	}
	else if (state == 3)
	{
		return GameOver;
	}

	return GameNotStart;
}
uint16_t getGameTime(void)
{
	return Game.Time;
}
uint16_t getGamestop(void)
{
    return Game.stop;
}
uint16_t getPassengerstartposX(void)
{
    return Passenger.startpos.X;
}
uint16_t getPassengerstartposY(void)
{
    return Passenger.startpos.Y;
}
struct Position getPassengerstartpos(void)
{
    return Passenger.startpos;
}
uint16_t getPassengerfinalposX(void)
{
    return Passenger.finalpos.X;
}
uint16_t getPassengerfinalposY(void)
{
    return Passenger.finalpos.Y;
}
struct Position getPassengerfinalpos(void)
{
    return Passenger.finalpos;
}
uint16_t getStopposX(int StopNo)
{
    if (StopNo != 0 && StopNo != 1)
		return (uint16_t)INVALID_ARG;
    else
        return Stop[StopNo].pos.X;
}
uint16_t getStopposY(int StopNo)
{
    if (StopNo != 0 && StopNo != 1)
		return (uint16_t)INVALID_ARG;
    else
        return Stop[StopNo].pos.Y;
}
struct Position getStoppos(int StopNo)
{
        return Stop[StopNo].pos;
}
uint16_t getCarposX(int CarNo)
{
    if (CarNo != 0 && CarNo != 1)
		return (uint16_t)INVALID_ARG;
	else
		return (uint16_t)Car[CarNo].pos.X;

}
uint16_t getCarposY(int CarNo)
{
    if (CarNo != 0 && CarNo != 1)
		return (uint16_t)INVALID_ARG;
	else
		return (uint16_t)Car[CarNo].pos.Y;
}
struct Position getCarpos(int CarNo)
{
		return Car[CarNo].pos;
}
uint16_t getPackageposX(int PackNo)
{
    if (PackNo != 0 && PackNo != 1 && PackNo != 2 && PackNo != 3 && PackNo != 4 && PackNo != 5)
		return (uint16_t)INVALID_ARG;
	else
		return (uint16_t)Package[PackNo].pos.X;
}
uint16_t getPackageposY(int PackNo)
{
    if (PackNo != 0 && PackNo != 1 && PackNo != 2 && PackNo != 3 && PackNo != 4 && PackNo != 5)
		return (uint16_t)INVALID_ARG;
	else
		return (uint16_t)Package[PackNo].pos.Y;
}
uint16_t getPackagewhetherpicked(int PackNo)
{
	if (PackNo != 0 && PackNo != 1 && PackNo != 2 && PackNo != 3 && PackNo != 4 && PackNo != 5)
		return (uint16_t)INVALID_ARG;
	else
		return (uint16_t)Package[PackNo].whetherpicked; 
} 
struct Position getPackagepos(int PackNo)
{
		return Package[PackNo].pos;
}
uint16_t getCarpicknum(int CarNo)
{
    if (CarNo != 0 && CarNo != 1)
		return (uint16_t)INVALID_ARG;
	else
		return (uint16_t)Car[CarNo].picknum;
}
uint16_t getCartransportnum(int CarNo)
{
    if (CarNo != 0 && CarNo != 1)
		return (uint16_t)INVALID_ARG;
	else
		return (uint16_t)Car[CarNo].transportnum;
}
uint16_t getCartransport(int CarNo)
{
    if (CarNo != 0 && CarNo != 1)
		return (uint16_t)INVALID_ARG;
	else
		return (uint16_t)Car[CarNo].transport;
}
uint16_t getCarscore(int CarNo)
{
    if (CarNo != 0 && CarNo != 1)
		return (uint16_t)INVALID_ARG;
	else
		return (uint16_t)Car[CarNo].score;
}
uint16_t getCartask(int CarNo)
{
    if (CarNo != 0 && CarNo != 1)
		return (uint16_t)INVALID_ARG;
	else
		return (uint16_t)Car[CarNo].task;
}
uint16_t getCararea(int CarNo)
{
    if (CarNo != 0 && CarNo != 1)
		return (uint16_t)INVALID_ARG;
	else
		return (uint16_t)Car[CarNo].area;
}
uint16_t getObstacleposX(void)
{
    return (uint16_t)Obstacle.X;
}
uint16_t getObstacleposY(void)
{
    return (uint16_t)Obstacle.Y;
}
struct Position getObstaclepos(void)
{
    return Obstacle;

}
uint16_t getCarcrossing(int CarNo)
{
    return (uint16_t)Car[CarNo].crossing;
}
/***************************************************/

void DecodeBasicInfo()
{
	Game.Time = (zigbeeReceive[0] << 8) + zigbeeReceive[1];
	Game.GameState = (zigbeeReceive[2] & 0xC0) >> 6;
	Game.stop=(zigbeeReceive[2]& 0x03);
}
void DecodeCarAInfo()
{
    Car[0].pos.X=(zigbeeReceive[3]);
    Car[0].pos.Y=(zigbeeReceive[4]);
    Car[0].score=(zigbeeReceive[28]<<8)+zigbeeReceive[29];
    Car[0].picknum=zigbeeReceive[34];
    Car[0].task=(zigbeeReceive[2] & 0x20>>5);
    Car[0].transport=(zigbeeReceive[2] & 0x08>>3);
    Car[0].transportnum=(zigbeeReceive[32]);
    Car[0].area=(zigbeeReceive[15] & 0x02>>1);
}
void DecodeCarBInfo()
{
    Car[1].pos.X=(zigbeeReceive[5]);
    Car[1].pos.Y=(zigbeeReceive[6]);
    Car[1].score=(zigbeeReceive[30]<<8)+zigbeeReceive[31];
    Car[1].picknum=zigbeeReceive[35];
    Car[1].task=(zigbeeReceive[2] & 0x08>>5);
    Car[1].transport=(zigbeeReceive[2] & 0x04>>3);
    Car[1].transportnum=(zigbeeReceive[33]);
    Car[1].area=(zigbeeReceive[15] & 0x01);
}
void DecodePassengerInfo()
{
    Passenger.startpos.X=(zigbeeReceive[11]);
    Passenger.startpos.Y=(zigbeeReceive[12]);
    Passenger.finalpos.X=(zigbeeReceive[13]);
    Passenger.finalpos.Y=(zigbeeReceive[14]);
}
void DecodePackageAInfo()
{
    Package[0].pos.X=(zigbeeReceive[16]);
    Package[0].pos.Y=(zigbeeReceive[17]);
    Package[0].whetherpicked=(zigbeeReceive[15] & 0x80>>7) 
}
void DecodePackageBInfo()
{
    Package[1].pos.X=(zigbeeReceive[18]);
    Package[1].pos.Y=(zigbeeReceive[19]);
    Package[0].whetherpicked=(zigbeeReceive[15] & 0x40>>7) 
}
void DecodePackageCInfo()
{
    Package[2].pos.X=(zigbeeReceive[20]);
    Package[2].pos.Y=(zigbeeReceive[21]);
    Package[0].whetherpicked=(zigbeeReceive[15] & 0x20>>7) 
}
void DecodePackageDInfo()
{
    Package[3].pos.X=(zigbeeReceive[22]);
    Package[3].pos.Y=(zigbeeReceive[23]);
    Package[0].whetherpicked=(zigbeeReceive[15] & 0x10>>7) 
}
void DecodePackageEInfo()
{
    Package[4].pos.X=(zigbeeReceive[24]);
    Package[4].pos.Y=(zigbeeReceive[25]);
    Package[0].whetherpicked=(zigbeeReceive[15] & 0x08>>7) 
}
void DecodePackageFInfo()
{
    Package[5].pos.X=(zigbeeReceive[26]);
    Package[5].pos.Y=(zigbeeReceive[27]);
    Package[0].whetherpicked=(zigbeeReceive[15] & 0x04>>7) 
}
void DecodeStopAInfo()
{
    Stop[0].pos.X=(zigbeeReceive[7]);
    Stop[0].pos.Y=(zigbeeReceive[8]);
}
void DecodeStopBInfo()
{
    Stop[1].pos.X=(zigbeeReceive[9]);
    Stop[1].pos.Y=(zigbeeReceive[10]);
}
void DecodeObstacle()
{
    Obstacle.X=(zigbeeReceive[36]);
    Obstacle.Y=(zigbeeReceive[37]);
}
void 
void DecodeAll()
{
	DecodeBasicInfo();
	DecodeCarAInfo();
	DecodeCarBInfo();
	DecodePassengerInfo();
	DecodePackageAInfo();
	DecodePackageBInfo();
	DecodePackageCInfo();
	DecodePackageDInfo();
	DecodePackageEInfo();
	DecodePackageFInfo();
	DecodeStopAInfo();
	DecodeStopBInfo();
}
int receiveIndexMinus(int index_h, int num)
{
	if (index_h - num >= 0)
	{
		return index_h - num;
	}
	else
	{
		return index_h - num + ZIGBEE_MESSAGE_LENTH;
	}
}

int receiveIndexAdd(int index_h, int num)
{
	if (index_h + num < ZIGBEE_MESSAGE_LENTH)
	{
		return index_h + num;
	}
	else
	{
		return index_h + num - ZIGBEE_MESSAGE_LENTH;
	}
}
