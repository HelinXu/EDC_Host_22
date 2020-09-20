zigbee
接收上位机的数据
接收说明（以USART2为例）
	在程序开始的时候使用zigbee_Init(&huart2)进行初始化;
	在回调函数中使用zigbeeMessageRecord(void)记录数据，并重新开启中断

数据说明
	struct BasicInfo Game; 储存比赛状态、时间、乘客数、小球有效性、小球位置
	struct CarInfo Car[2]; 储存车辆信息
	struct PassengerInfo Passenger[2]: 储存乘客的序号和位置
	通过接口获取数据	
