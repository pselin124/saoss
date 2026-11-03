#include "Sets.h"
//#include "h/main.h"
#include "Define.h"
#include <time.h>

#ifndef WIN32
  #define itoa(a, b, c) snprintf(b, c, "%d", a)
#endif

Sets::Sets(){
	fileluettu=false;
	width=640;
	height=480;
	efects=0;
	musics=0;
	fullscreen=0;
	credits=200;
	ai=0;
	fps=100;
	musicvolume=100;
	soundvolume=100;
	playermaara=2;
	ihminen[0]=true;
	ihminen[1]=true;
	ihminen[2]=true;
	ihminen[3]=true;
	ihminen[4]=false;
	ihminen[5]=false;
	ihminen[6]=false;
	ihminen[7]=false;
	ihminen[8]=false;
	ihminen[9]=false;
	ihminen[10]=false;
	ihminen[11]=false;
	ihminen[12]=false;
	ihminen[13]=false;
	ihminen[14]=false;
	ihminen[15]=false;
	int i;
	for(i=0;i<maxplayers;i++){
		team[i]=0;
	}
	aikohteet=false;
	sprintf(nick[0],"Player 1");
	sprintf(nick[1],"Player 2");
	sprintf(nick[2],"Player 3");
	sprintf(nick[3],"Player 4");
	cammode=0;
	detail=3;
	maxdetail=5;
	mode256=false;
}
int Sets::saveFile(){
	printf("pl:%i",playermaara);
	if ((ini=fopen("sets.txt", "wb"))==NULL){
		return(0);
	}
	int i;
	char *rivi[]={	";\r\n",
					"screenwidth=",
					"screenheight=",
					"soundeffects=",
					"musics=",
					"fullscreen=",
					"credits=",
					"testopponent=",
					"fps=",
					"music volume=",
					"sound volume=",
					"players=",
					"ai vs ai=",
					"nick1=",
					"nick2=",
					"nick3=",
					"nick4=",
					"cammode=",
					"detail=",
					"mode256="
					};
	
	char numero[20][10];
	int j;
	for(i=0;i<20;i++) 
		for(j=0;j<10;j++) numero[i][j]='\0';

	itoa(width			, numero[1], 10);
	itoa(height			, numero[2], 10);
	itoa(efects			, numero[3], 10);
	itoa(musics			, numero[4], 10);
	itoa(fullscreen		, numero[5], 10);
	itoa((int)credits	, numero[6], 10);
	itoa(ai				, numero[7], 10);
	itoa(fps			, numero[8], 10);
	itoa((int)musicvolume	, numero[9], 10);
	itoa((int)soundvolume	, numero[10], 10);
	itoa(playermaara		, numero[11], 10);
	if(aikohteet)
		itoa(1		, numero[12], 10);
	else 
		itoa(0		, numero[12], 10);
	for(i=0;i<4;i++){
		sprintf(numero[13+i],nick[i]);
	}
	itoa(cammode		, numero[17], 10);
	itoa(detail		, numero[18], 10);
	if(mode256) itoa(1, numero[19], 10);
	else itoa(0, numero[19], 10);

	for(i=1;i<20;i++){
		fwrite(rivi[i], 1, strlen(rivi[i]), ini);
//		if(i==13) fwrite(nick, 1, strlen( nick), ini);
		//else 
			fwrite(numero[i], 1, strlen( numero[i]), ini);
		fwrite(rivi[0], 1, strlen(rivi[0]), ini);
	}

	for(i=0;i<maxplayers;i++){
		fwrite("player", 1, strlen("player"), ini);  //player
		itoa(i+1, numero[1], 10);
		fwrite(numero[1], 1, strlen( numero[1]), ini); // i
		fwrite("ihmispelaaja=", 1, strlen("ihmispelaaja="), ini); //ihmispelaaja=
		if(ihminen[i]==false)
			itoa(0, numero[1], 10); //konepelaaja -> 0
		else{
			itoa(1, numero[1], 10);  //ihmispelaaja ->1
		}
		fwrite(numero[1], 1, strlen( numero[1]), ini); // 0 tai 1
		fwrite(rivi[0], 1, strlen(rivi[0]), ini);
	}

	for(i=0;i<maxplayers;i++){
		fwrite("player", 1, strlen("player"), ini);  //player
		itoa(i+1, numero[1], 10);
		fwrite(numero[1], 1, strlen( numero[1]), ini); // i
		fwrite("team=", 1, strlen("team="), ini); //team=
		itoa(team[i]+1, numero[1], 10); //team-numero
		fwrite(numero[1], 1, strlen( numero[1]), ini); 
		fwrite(rivi[0], 1, strlen(rivi[0]), ini);
	}

	fwrite("[]\r\n",1,strlen("[]\r\n"), ini);


	fclose(ini);

	return 1;
}
int Sets::loadFile(char *filename){
	char *array;
	int kohta, size;
	char rivi[40];
	int i;
	for(i=0;i<40;i++){
		rivi[i]='\0';
	}
	filee=filename;
	if ((ini=fopen(filename, "rb"))==NULL){
		return(0);
	}
	fseek(ini, 0, SEEK_END);
	size = ftell(ini);
	fseek(ini, 0, SEEK_SET);
	array = new char[size];
	fread(array, size, 1, ini);
	kohta=0;
/*	printf(array);
	printf("\n");*/
	while(!fileluettu){
		//fileluettu=true;
		char edellinen='\0';
		char nykyinen;
		for (i=0;i<40;i++){
			rivi[i]='\0';
		}
		int j=0;
		for(i=kohta;i<size;i++,j++){
			nykyinen=array[i];
			if(i>kohta) edellinen=array[i-1];
			if(i==size-1) kohta=i;
			else if(array[i]==']'){
				i=size;
				kohta=size;
			}
			else if(array[i]==';' || (array[i]==0x0a && edellinen==0x0d)){
				if(array[i]==';' && array[i+2]!=0x0d) kohta=i+2;
				else kohta=i+3;
				i=size;
				rivi[j]='\0';
			}else rivi[j]=array[i];
		}
		if(kohta>=size-1) {
			fileluettu=true;
			rivi[j]='\0';
		}
		//printf("%s\n",rivi);
		//printf("efects=%i\n",efects);
		if(sisaltaa(rivi,"nick1=",5)){
			nimi(nick[0],rivi);
		}
		if(sisaltaa(rivi,"nick2=",5)){
			nimi(nick[1],rivi);
		}
		if(sisaltaa(rivi,"nick3=",5)){
			nimi(nick[2],rivi);
		}
		if(sisaltaa(rivi,"nick4=",5)){
			nimi(nick[3],rivi);
		}
		if(sisaltaa(rivi,"screenwidth", 10)){
			width=arvo(rivi);
		}
		if(sisaltaa(rivi,"cammode", 6)){
			cammode=arvo(rivi);
		}
		if(sisaltaa(rivi,"screenheight", 10)){
			height=arvo(rivi);
		}
		if(sisaltaa(rivi,"soundeffects",7)){
			efects=arvo(rivi);
		}
		if(sisaltaa(rivi,"musics",6)){
			musics=arvo(rivi);
		}
		if(sisaltaa(rivi,"detail",6)){
			detail=arvo(rivi);
			if(detail<0) detail=0;
			if(detail>maxdetail) detail=maxdetail;
			sirpaleita=1000+detail*500;
			alkusirpaleita=(int) (sirpaleita/12.5f);
		}
		if(sisaltaa(rivi,"mode256",7)){
			int juttu=arvo(rivi,8);
			if(juttu>0) mode256=true;
			else mode256=false;
			printf("juttu:%i\n",juttu);
		}
		if(sisaltaa(rivi,"ai vs ai",6)){
			if(arvo(rivi)>0) aikohteet=1;
			else aikohteet=0;
		}
		if(sisaltaa(rivi,"player1team",9)){
			team[0]=arvo(rivi,9);
			team[0]--;
			if(team[0]>=maxplayers)team[0]=maxplayers-1;
			if(team[0]<0) team[0]=0;
		}
		if(sisaltaa(rivi,"player2team",9)){
			team[1]=arvo(rivi,9);
			team[1]--;
			if(team[1]>=maxplayers)team[1]=maxplayers-1;
			if(team[1]<0) team[1]=0;
		}
		if(sisaltaa(rivi,"player3team",9)){
			team[2]=arvo(rivi,9);
			team[2]--;
			if(team[2]>=maxplayers)team[2]=maxplayers-1;
			if(team[2]<0) team[2]=0;
		}
		if(sisaltaa(rivi,"player4team",9)){
			team[3]=arvo(rivi,9);
			team[3]--;
			if(team[3]>=maxplayers)team[3]=maxplayers-1;
			if(team[3]<0) team[3]=0;
		}
		if(sisaltaa(rivi,"player5team",9)){
			team[4]=arvo(rivi,9);
			team[4]--;
			if(team[4]>=maxplayers)team[4]=maxplayers-1;
			if(team[4]<0) team[4]=0;
		}
		if(sisaltaa(rivi,"player6team",9)){
			team[5]=arvo(rivi,9);
			team[5]--;
			if(team[5]>=maxplayers)team[5]=maxplayers-1;
			if(team[5]<0) team[5]=0;
		}
		if(sisaltaa(rivi,"player7team",9)){
			team[6]=arvo(rivi,9);
			team[6]--;
			if(team[6]>=maxplayers)team[6]=maxplayers-1;
			if(team[6]<0) team[6]=0;
		}
		if(sisaltaa(rivi,"player8team",9)){
			team[7]=arvo(rivi,9);
			team[7]--;
			if(team[7]>=maxplayers)team[7]=maxplayers-1;
			if(team[7]<0) team[7]=0;
		}
		if(sisaltaa(rivi,"player9team",9)){
			team[8]=arvo(rivi,9);
			team[8]--;
			if(team[8]>=maxplayers)team[8]=maxplayers-1;
			if(team[8]<0) team[8]=0;
		}
		if(sisaltaa(rivi,"player10team",9)){
			team[9]=arvo(rivi,9);
			team[9]--;
			if(team[9]>=maxplayers)team[9]=maxplayers-1;
			if(team[9]<0) team[9]=0;
		}

		if(sisaltaa(rivi,"player1ihmis",10)){
			if(arvo(rivi,10)>0){
//				printf("ykkönenihminen\n");
				ihminen[0]=true;
			}
			else{
				ihminen[0]=false;
//				printf("ykkönen ei ihminen\n");
			}
		}
		if(sisaltaa(rivi,"player2ihmis",9)){
			if(arvo(rivi,9)>0) ihminen[1]=true;
			else ihminen[1]=false;
		}
		if(sisaltaa(rivi,"player3ihmis",9)){
			if(arvo(rivi,9)>0) ihminen[2]=true;
			else ihminen[2]=false;
		}
		if(sisaltaa(rivi,"player4ihmis",9)){
			if(arvo(rivi,9)>0) ihminen[3]=true;
			else ihminen[3]=false;
		}
		if(sisaltaa(rivi,"sound volume",7)){
			soundvolume=(float)arvo(rivi);
			if(soundvolume<0)soundvolume=0;
			if(soundvolume>100)soundvolume=100;
		}
		if(sisaltaa(rivi,"music volume",10)){
			musicvolume=(float)arvo(rivi);
	//		musicvolume=atoi(&rivi[12]);
			if(musicvolume<0)musicvolume=0;
			if(musicvolume>100)musicvolume=100;
		}
		if(sisaltaa(rivi,"credits",6)){
			credits=arvo(rivi);
			if(credits<20) credits=20;
			if(credits>1000) credits=1000;
		}

		if(sisaltaa(rivi,"fullscreen",8)){
			fullscreen=arvo(rivi);
		}
		if(sisaltaa(rivi,"test",4)){
			ai=arvo(rivi);
		}
		if(sisaltaa(rivi,"fps",3)){
			fps=arvo(rivi);
			if(fps<50) fps=50;
			if(fps>200) fps=200;
		}
		if(sisaltaa(rivi,"players",7)){
			playermaara=arvo(rivi);
			if(playermaara>maxplayers)playermaara=maxplayers;
			if(playermaara<2)playermaara=2;
		}

	}
	fclose(ini);
	delete array;
	return(1);
}
bool Sets::sisaltaa(char *data, char *teksti, int pituus){
	int pointer=0;
	int i;
	for(i=0;i<40;i++){
		if(pointer>=pituus) return true;
		else if(data[i]==teksti[pointer]){
			pointer++;
		} else{
			pointer=0;
		}
	}
	return false;
}

void Sets::nimi(char *nick, char *data){
	int i;
	for(i=0;i<21;i++){
		nick[i]='\0';
	}
	int pointer=0;
	pointer=7;
	int loppu=0;
	for(i=0;i<21;i++){
		if(data[i+pointer]=='\0'){
			loppu=i;
		}
		else {
			nick[i]=data[i+pointer];
			if(nick[i]>='A' && nick[i]<='Z') nick[i]-='A'-'a';
		}
	}
	for(i=loppu;i<9;i++){
		nick[i]='\0';
	}
	//printf("nikki=%s\n",nick);
}

int Sets::arvo(char *data, int kohta){
	int i;
	char numero[10];
	for(i=0;i<10;i++){
		numero[i]='\0';
	}
	int pointer=0;
	for(i=kohta;i<40;i++){
		if(data[i]>='0' && data[i]<='9'){
			if(pointer==0){// && data[i-1]=='='){
				numero[pointer]=data[i];
				pointer++;
			}else if(data[i-1]>='0' && data[i-1]<='9'){
				numero[pointer]=data[i];
				pointer++;
			}else pointer=0;
		}
		if(pointer>8) i=40;
	}
	return atoi(numero);
}

int	Sets::arvo(char *data){
	int i;
	char numero[10];
	for(i=0;i<10;i++){
		numero[i]='\0';
	}
	int pointer=0;
	for(i=0;i<40;i++){
		if(data[i]>='0' && data[i]<='9'){
			if(pointer==0){// && data[i-1]=='='){
				numero[pointer]=data[i];
				pointer++;
			}else if(data[i-1]>='0' && data[i-1]<='9'){
				numero[pointer]=data[i];
				pointer++;
			}else pointer=0;
		}
		if(pointer>8) i=40;
	}
	return atoi(numero);
}

void Sets::freadline(){
/*	char edellinen='\0';
	char nykyinen;
	int i;
	for (i=0;i<40;i++){
		rivi[i]='\0';
	}
	int j=0;
	for(i=kohta;i<size;i++,j++){
//		nykyinen=array[i];
//		if(i>kohta) edellinen=array[i-1];
		if(array[i]==';'){// || (array[i]==0x0a && edellinen==0x0d)){
			kohta=i;
			rivi[j]='\0';
		}else rivi[j]=array[i];
	}
	if(kohta>=size-1 || i>=size-1) {
		fileluettu=true;
		rivi[j]='\0';
	}*/
}
