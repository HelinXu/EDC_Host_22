/********************************
zigbee.h
������λ��������
����˵������USART2Ϊ����
	�ڳ���ʼ��ʱ��ʹ��zigbee_Init(&huart2)���г�ʼ��;
	�ڻص�������ʹ��zigbeeMessageRecord(void)��¼���ݣ������¿����ж�

����˵��
    struct BasicInfo Game;�������״̬��ʱ�䡢й�����Ϣ
    struct CarInfo CarInfo[2];//���泵����Ϣ
    struct PassengerInfo Passenger;//������Ա����Ϣ��λ�ú��ʹ�λ��
    struct PackageInfo Package[6];//�����Ѵ���ʵ���Ϣ
    struct StopInfo Stop[2];//����й���λ����Ϣ
    struct Position Obstacle;//���������ϰ���Ϣ
    ͨ���ӿڻ�ȡ����
**********************************/
#ifndef V0_5_H_INCLUDED
#define V0_5_H_INCLUDED
#include "stm32f1xx_hal.h"

#define INVALID_ARG -1
#define ZIGBEE_MESSAGE_LENTH 40


struct Position
{
    unsigned int X;
    unsigned int Y;
};

struct BasicInfo
{
    uint8_t GameState;	//��Ϸ״̬��00δ��ʼ��01�����У�10��ͣ��11����
    uint16_t Time;	//����ʱ�䣬��0.1sΪ��λ
    uint8_t stop;   //й��ڿ�����Ϣ
};
struct CarInfo
{
    uint8_t No;     //������ţ�AΪ0��BΪ1
    struct Position pos;    //С��λ��
    uint16_t score;         //�÷�
    uint8_t picknum;         //С���ɹ��ռ����ʸ���
    uint8_t task;           //С������0�ϰ볡��1�°볡
    uint8_t transport;         //С�����Ƿ�����
    uint8_t transportnum;      //С�������˵ĸ���
    uint8_t area;    //С�����ڵ�����
};
struct PassengerInfo
{
    struct Position startpos;   //��Ա��ʼλ��
    struct Position finalpos;   //��ԱҪ�����λ��
};
struct PackageInfo
{
    uint8_t No;               //���ʱ��
    struct Position pos;
    uint8_t whetherpicked;    //�����Ƿ��ѱ�ʰȡ 
};
struct StopInfo
{
    uint8_t No;
    struct Position pos;
};
enum GameStateEnum
{
	GameNotStart,	//δ��ʼ
	GameGoing,		//������
	GamePause,		//��ͣ��
	GameOver			//�ѽ���
};
/**************�ӿ�*************************/
void zigbee_Init(UART_HandleTypeDef *huart);//��ʼ��
void zigbeeMessageRecord(void);							//ʵʱ��¼��Ϣ����ÿ�ν�����ɺ�������ݣ����¿����ж�

enum GameStateEnum getGameState(void);			//����״̬
uint16_t getGameTime(void);	                  //����ʱ�䣬��λΪ0.1s
uint16_t getPassengerstartposX(void);			//��Ա��ʼλ��
uint16_t getPassengerstartposY(void);
struct Position getPassengerstartpos(void);
uint16_t getPassengerfinalposX(void);           //��Ա�赽��λ��
uint16_t getPassengerfinalposY(void);
struct Position getPassengerfinalpos(void);
uint16_t getGamestop(void);               //й��ڿ�����Ϣ
uint16_t getStopposX(int StopNo);			//й���λ��X
uint16_t getStopposY(int StopNo);           //й���λ��Y
struct Position getStoppos(int StopNo);     //й���λ��
uint16_t getCarposX(int CarNo);		    //С��x����
uint16_t getCarposY(int CarNo);			//С��y����
struct Position getCarpos(int CarNo);	//С��λ��
uint16_t getPackageposX(int PackNo);		    //����x����
uint16_t getPackageposY(int PackNo);			//����y����
uint16_t getPackagewhetherpicked(int PackNo);   //�����Ƿ��ѱ��ռ� 
struct Position getPackagepos(int PackNo);	//����λ��
uint16_t getCarpicknum(int CarNo);//С���ռ���
uint16_t getCartransportnum(int CarNo);//С��������Ա��
uint16_t getCartransport(int CarNo);//С���Ƿ�����������Ա
uint16_t getCarscore(int CarNo);//С���÷�
uint16_t getCartask(int CarNo);//С������
uint16_t getCararea(int CarNo);//С������
uint16_t getObstacleposX(void);		    //�����ϰ�x����
uint16_t getObstacleposY(void);			//�����ϰ�y����
struct Position getObstaclepos(void);	//�����ϰ�λ��
#endif // V0_5_H_INCLUDED
