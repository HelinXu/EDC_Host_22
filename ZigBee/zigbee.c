#include"zigbee.h"
volatile uint8_t zigbeeReceive[ZIGBEE_MESSAGE_LENTH];	//实时记录收到的信息
volatile uint8_t zigbeeMessage[ZIGBEE_MESSAGE_LENTH];//经过整理顺序后得到的信息
volatile int message_index = 0;
volatile int message_head = -1;
uint8_t zigbeeBuffer[1];

UART_HandleTypeDef* zigbee_huart;


volatile struct BasicInfo Game;//储存比赛状态、时间、泄洪口信息
volatile struct CarInfo Car;//储存车辆信息
volatile struct PassengerInfo Passenger;//储存人员的信息、位置和送达位置
volatile struct PackageInfo Package[6];//储存防汛物资的信息
volatile struct FloodInfo Flood[2];//储存泄洪口位置信息
volatile struct ObstacleInfo Obstacle[16];//储存虚拟障碍信息
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
uint16_t getFloodposX(int FloodNo)
{
    if (FloodNo != 0 && FloodNo != 1)
		return (uint16_t)INVALID_ARG;
    else
        return Flood[FloodNo].pos.X;
}
uint16_t getFloodposY(int FloodNo)
{
    if (FloodNo != 0 && FloodNo != 1)
		return (uint16_t)INVALID_ARG;
    else
        return Flood[FloodNo].pos.Y;
}
struct Position getFloodpos(int FloodNo)
{
        return Flood[FloodNo].pos;
}
uint16_t getCarposX()
{
		return (uint16_t)Car[CarNo].pos.X;

}
uint16_t getCarposY()
{
		return (uint16_t)Car[CarNo].pos.Y;
}
struct Position getCarpos()
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
uint16_t getCarpicknum()
{
		return (uint16_t)Car.picknum;
}
uint16_t getCartransportnum()
{
		return (uint16_t)Car.transportnum;
}
uint16_t getCartransport()
{
		return (uint16_t)Car.transport;
}
uint16_t getCarscore()
{
		return (uint16_t)Car.score;
}
uint16_t getCartask()
{
        return (uint16_t)Car.task;
}
uint16_t getCararea()
{
		return (uint16_t)Car.area;
}
uint16_t getObstacleAposX(int ObstacleNo)		    //虚拟障碍Ax坐标
{
    return (uint16_t)Obstacle[ObstacleNo].posA.X;
}
uint16_t getObstacleAposY(int ObstacleNo)		    //虚拟障碍Ax坐标
{
    return (uint16_t)Obstacle[ObstacleNo].posA.Y;
}
uint16_t getObstacleBposX(int ObstacleNo)		    //虚拟障碍Ax坐标
{
    return (uint16_t)Obstacle[ObstacleNo].posB.X;
}
uint16_t getObstacleBposY(int ObstacleNo)	    //虚拟障碍Ax坐标
{
    return (uint16_t)Obstacle[ObstacleNo].posB.Y;
}
struct Position getObstacleApos(int ObstacleNo)
{
    return Obstacle[ObstacleNo].posA;
}
struct Position getObstacleBpos(int ObstacleNo)
{
    return Obstacle[ObstacleNo].posB;
}
/***************************************************/

void DecodeBasicInfo()
{
	Game.Time = (zigbeeReceive[0] << 8) + zigbeeReceive[1];
	Game.GameState = (zigbeeReceive[2] & 0xC0) >> 6;
	Game.stop=(zigbeeReceive[2]& 0x03);
}
void DecodeCarInfo()
{
    Car.pos.X=(zigbeeReceive[3]);
    Car.pos.Y=(zigbeeReceive[4]);
    Car.score=(zigbeeReceive[28]<<8)+zigbeeReceive[29];
    Car.picknum=zigbeeReceive[34];
    Car.task=(zigbeeReceive[2] & 0x20>>5);
    Car.transport=(zigbeeReceive[2] & 0x08>>3);
    Car.transportnum=(zigbeeReceive[32]);
    Car.area=(zigbeeReceive[15] & 0x02>>1);
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
    Package[0].whetherpicked=(zigbeeReceive[15] & 0x80>>7);
}
void DecodePackageBInfo()
{
    Package[1].pos.X=(zigbeeReceive[18]);
    Package[1].pos.Y=(zigbeeReceive[19]);
    Package[0].whetherpicked=(zigbeeReceive[15] & 0x40>>7);
}
void DecodePackageCInfo()
{
    Package[2].pos.X=(zigbeeReceive[20]);
    Package[2].pos.Y=(zigbeeReceive[21]);
    Package[0].whetherpicked=(zigbeeReceive[15] & 0x20>>7);
}
void DecodePackageDInfo()
{
    Package[3].pos.X=(zigbeeReceive[22]);
    Package[3].pos.Y=(zigbeeReceive[23]);
    Package[0].whetherpicked=(zigbeeReceive[15] & 0x10>>7);
}
void DecodePackageEInfo()
{
    Package[4].pos.X=(zigbeeReceive[24]);
    Package[4].pos.Y=(zigbeeReceive[25]);
    Package[0].whetherpicked=(zigbeeReceive[15] & 0x08>>7);
}
void DecodePackageFInfo()
{
    Package[5].pos.X=(zigbeeReceive[26]);
    Package[5].pos.Y=(zigbeeReceive[27]);
    Package[0].whetherpicked=(zigbeeReceive[15] & 0x04>>7);
}
void DecodeFloodAInfo()
{
    Flood[0].pos.X=(zigbeeReceive[7]);
    Flood[0].pos.Y=(zigbeeReceive[8]);
}
void DecodeFloodBInfo()
{
    Flood[1].pos.X=(zigbeeReceive[9]);
    Flood[1].pos.Y=(zigbeeReceive[10]);
}
void DecodeObstacle()
{
    Obstacle[0].posA.X=(zigbeeReceive[32]);
    Obstacle[0].posA.Y=(zigbeeReceive[33]);
    Obstacle[0].posB.X=(zigbeeReceive[34]);
    Obstacle[0].posB.Y=(zigbeeReceive[35]);
    Obstacle[1].posA.X=(zigbeeReceive[36]);
    Obstacle[1].posA.Y=(zigbeeReceive[37]);
    Obstacle[1].posB.X=(zigbeeReceive[38]);
    Obstacle[1].posB.Y=(zigbeeReceive[39]);
    Obstacle[2].posA.X=(zigbeeReceive[40]);
    Obstacle[2].posA.Y=(zigbeeReceive[41]);
    Obstacle[2].posB.X=(zigbeeReceive[42]);
    Obstacle[2].posB.Y=(zigbeeReceive[43]);
    Obstacle[3].posA.X=(zigbeeReceive[44]);
    Obstacle[3].posA.Y=(zigbeeReceive[45]);
    Obstacle[3].posB.X=(zigbeeReceive[46]);
    Obstacle[3].posB.Y=(zigbeeReceive[47]);
    Obstacle[4].posA.X=(zigbeeReceive[48]);
    Obstacle[4].posA.Y=(zigbeeReceive[49]);
    Obstacle[4].posB.X=(zigbeeReceive[50]);
    Obstacle[4].posB.Y=(zigbeeReceive[51]);
    Obstacle[5].posA.X=(zigbeeReceive[52]);
    Obstacle[5].posA.Y=(zigbeeReceive[53]);
    Obstacle[5].posB.X=(zigbeeReceive[54]);
    Obstacle[5].posB.Y=(zigbeeReceive[55]);
    Obstacle[6].posA.X=(zigbeeReceive[56]);
    Obstacle[6].posA.Y=(zigbeeReceive[57]);
    Obstacle[6].posB.X=(zigbeeReceive[58]);
    Obstacle[6].posB.Y=(zigbeeReceive[59]);
    Obstacle[7].posA.X=(zigbeeReceive[60]);
    Obstacle[7].posA.Y=(zigbeeReceive[61]);
    Obstacle[7].posB.X=(zigbeeReceive[62]);
    Obstacle[7].posB.Y=(zigbeeReceive[63]);
    Obstacle[8].posA.X=(zigbeeReceive[64]);
    Obstacle[8].posA.Y=(zigbeeReceive[65]);
    Obstacle[8].posB.X=(zigbeeReceive[66]);
    Obstacle[8].posB.Y=(zigbeeReceive[67]);
    Obstacle[9].posA.X=(zigbeeReceive[68]);
    Obstacle[9].posA.Y=(zigbeeReceive[69]);
    Obstacle[9].posB.X=(zigbeeReceive[70]);
    Obstacle[9].posB.Y=(zigbeeReceive[71]);
    Obstacle[10].posA.X=(zigbeeReceive[72]);
    Obstacle[10].posA.Y=(zigbeeReceive[73]);
    Obstacle[10].posB.X=(zigbeeReceive[74]);
    Obstacle[10].posB.Y=(zigbeeReceive[75]);
    Obstacle[11].posA.X=(zigbeeReceive[76]);
    Obstacle[11].posA.Y=(zigbeeReceive[77]);
    Obstacle[11].posB.X=(zigbeeReceive[78]);
    Obstacle[11].posB.Y=(zigbeeReceive[79]);
    Obstacle[12].posA.X=(zigbeeReceive[80]);
    Obstacle[12].posA.Y=(zigbeeReceive[81]);
    Obstacle[12].posB.X=(zigbeeReceive[82]);
    Obstacle[12].posB.Y=(zigbeeReceive[83]);
    Obstacle[13].posA.X=(zigbeeReceive[84]);
    Obstacle[13].posA.Y=(zigbeeReceive[85]);
    Obstacle[13].posB.X=(zigbeeReceive[86]);
    Obstacle[13].posB.Y=(zigbeeReceive[87]);
    Obstacle[14].posA.X=(zigbeeReceive[88]);
    Obstacle[14].posA.Y=(zigbeeReceive[89]);
    Obstacle[14].posB.X=(zigbeeReceive[90]);
    Obstacle[14].posB.Y=(zigbeeReceive[91]);
    Obstacle[15].posA.X=(zigbeeReceive[92]);
    Obstacle[15].posA.Y=(zigbeeReceive[93]);
    Obstacle[15].posB.X=(zigbeeReceive[94]);
    Obstacle[15].posB.Y=(zigbeeReceive[95]);

}
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
	DecodeFloodAInfo();
	DecodeFloodBInfo();
	DecodeObstacle();
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

