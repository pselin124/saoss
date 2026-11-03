#include "main.h"
#include "net.h"
#include "chat.h"

void chatScreen(){
		if(hostpeli) printf("hostpeli2\n");
	SDL_EnableUNICODE(1);
	gamequited=false;
	bool menuready=false , connect=false , exitmenu=false;
	entterpressed=false; spacepressed=false;
	player[0].pfire=false; player[0].palas=false; player[0].pright=false; player[0].pleft=false;
	int selectionmax=2;	int selection=0;
	started2=0;
	bool kirjotusmode=false;
	luettunappi=0;

	tyhjennaKirjotusBufferi();
	while(!started2 && !exitmenu && !gamequited){
		while(!menuready){
			processEvents();
			if(started2 || gamequited) menuready=true;
			if(entterpressed && !kirjotusmode){
				menuready=true;
			}else if(escpressed){
				if(kirjotusmode){
					menuready=true;
				}else{
					menuready=true;
					selection=selectionmax;
				}
			}else if(player[0].pylos || player[0].pright
				|| player[0].palas || player[0].pleft){
				if(!kirjotusmode){
				  if(selection==0) selection=2;
				  else selection=0;
					player[0].pylos=false;
					player[0].pright=false;
					player[0].palas=false;
					player[0].pleft=false;
				}
			}else if(entterpressed && kirjotusmode){
				tekstiValmis();
				entterpressed=false;
			}else if(kirjotusmode){
				if(luetaan>1){
					luetaan=1;

					printf("kirjotettu %c,%i\n",luettunappi,luettunappi);
//					if(luettunappi>='A' && luettunappi<='Z') luettunappi-='A'-'a';
					if( (sdlnappi == SDLK_v && modifier&KMOD_CTRL) ||
						(sdlnappi == SDLK_INSERT)){// && modifier&KMOD_SHIFT)){
#ifdef WIN32
						OpenClipboard(NULL);
						HANDLE h = GetClipboardData(CF_TEXT);
						GlobalLock(h);
						int koko=strlen((const char*)h);
						if(koko+kirjotettavakirjain>=MAXCHATKIRJOTUSMERKIT-1) 
							koko=MAXCHATKIRJOTUSMERKIT-kirjotettavakirjain-2;
						if(koko>0) strncpy(&kirjotusbuffer[kirjotettavakirjain],(const char*)h,koko);
						else kirjotettavakirjain+=koko;
						GlobalUnlock(h);
						CloseClipboard();
#endif
					}
					else if(luettunappi==8){
						if(kirjotettavakirjain>0){
							kirjotettavakirjain--;
							vikakirjain--;
							kirjotusbuffer[kirjotettavakirjain]='\0';
						}
					}
					else {
/*						if((luettunappi>='a' && luettunappi<='z') 
							|| luettunappi=='.' 
							|| luettunappi==',' 
							|| (luettunappi>='A' && luettunappi<='Z') 
							|| (luettunappi>='0' && luettunappi<='9')
							|| luettunappi=='-'
							|| luettunappi==' '){*/
						kirjotusbuffer[kirjotettavakirjain]=luettunappi;
						kirjotusbuffer[kirjotettavakirjain+1]='\0';
						if(kirjotettavakirjain<MAXCHATKIRJOTUSMERKIT-2) kirjotettavakirjain++;
//						printf("kirjotettu %c,buf=%c,num=%i\n",luettunappi,kirjotusbuffer[kirjotettavakirjain],kirjotettavakirjain);
					}

					if(kirjotettavakirjain-vasenkirjain>ruudunleveys)
						vasenkirjain=kirjotettavakirjain-ruudunleveys;
					if(vasenkirjain>kirjotettavakirjain) 
						vasenkirjain=kirjotettavakirjain;
					if(vasenkirjain>kirjotettavakirjain-10 && kirjotettavakirjain-10>=0)
						vasenkirjain=kirjotettavakirjain-10;
				}
//int vikakirjain, kirjotettavakirjain, vasenkirjain, ruudunleveys=70;
//char chatbuffer[MAXCHATRIVIT][MAXCHATMERKIT];
//char kirjotusbuffer[MAXCHATKIRJOTUSMERKIT];

			}
			do{
				currenttime=SDL_GetTicks()-starttime;
				framecounter=calculatefps*currenttime/1000.0;
				if(oldframecounter==framecounter) SDL_Delay(5);
			}	while (oldframecounter==framecounter);
			oldframecounter=framecounter;
			if(joinpeli) kuuntelejoin();
			else{//hostpeli
				paivitaPelaajat();
				kuuntele();
				lahettele();
			}
			drawChatScreen(selection);
			updateAudio();
			player[0].pylos=false;
			player[0].pleft=false;
			player[0].pright=false;
			player[0].palas=false;
		}
		if(!kirjotusmode && selection==2){
			exitmenu=true;
		}else if(kirjotusmode){
			tyhjennaKirjotusBufferi();
			vasenkirjain=0;
			kirjotettavakirjain=0;
			kirjotusmode=false;
			selection=0;
			luetaan=0;
		}else if(selection==0){
			tyhjennaKirjotusBufferi();
			vasenkirjain=0;
			kirjotettavakirjain=0;
			kirjotusmode=true;
			luetaan=1;
			selection=1;
		}

		menuready=false;
		entterpressed=false;
		spacepressed=false;
		escpressed=false;
	}
}

void drawChatScreen(int selection){
	startMenu();
	glPushMatrix();
		glColor4f(1,1,1,1);
		glBindTexture(GL_TEXTURE_2D, texture[18]);				
		glPushMatrix();
			if(selection==0) glTranslatef(-3.6f,-4.75f,0);
			else if(selection==1) glTranslatef(-7.8f,-3.95f,0);
			else if(selection==2) glTranslatef( 1.4f,-4.75f,0);
			float tempkoko=0.6;
			glBegin(GL_QUADS);
				glNormal3f(0,0,1.0f);
				glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*tempkoko, -1.0f*tempkoko, 0);
				glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*tempkoko, -1.0f*tempkoko, 0);
				glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*tempkoko,  1.0f*tempkoko, 0);
				glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*tempkoko,  1.0f*tempkoko, 0);
			glEnd();
		glPopMatrix();
		glColor4f(1,0,0,1);
		char *vekotin[]={ "write",
											"exit"};
					

		gl_println( -15,  -24, -60,vekotin[0]);
		gl_println(  10,  -24, -60,vekotin[1]);
		gl_println( -35,  -20, -60,&kirjotusbuffer[vasenkirjain],0.8);

		int i;
		for(i=0;i<MAXCHATRIVIT;i++){
			gl_println( -36,  -17+i*2.0f, -60,chatbuffer[i],0.8);
		}


		glPopMatrix();
	stopMenu();
}



void lahetaRiviKlienteille(char *teksti, int pituus){
	int j;
	char msg[516];
	sprintf(msg,"xät");
	memcpy(&msg[3],sets->nick[0],10);
	for(j=3;j<13;j++){
		if(msg[j]=='\0')msg[j]=' ';
	}
	int koko=pituus;
	if(koko>500)koko=500;
	int n=13;
	msg[n]=':';
	n++;
	memcpy(&msg[n],teksti,koko);
	n+=koko;

	msg[n]=';';
	n++;
	msg[n]='\0';
	n++;
	int len=n;

	int i;
	for(i=1;i<MAXNETPLAYERS;i++){
		if(tcpsoketit[i]!=NULL && tila[i]>=0){
			int result=SDLNet_TCP_Send(tcpsoketit[i],msg,len);
			if(result<len) {
				printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
			}
		}
	}

}
void lahetaRiviHostille(char *teksti, int pituus){
	int i;
	char msg[516];
	sprintf(msg,"xät");
	memcpy(&msg[3],sets->nick[0],10);
	for(i=3;i<13;i++){
		if(msg[i]=='\0')msg[i]=' ';
	}
	int koko=pituus;
	if(koko>500)koko=500;
	int n=13;
	msg[n]=':';
	n++;
	memcpy(&msg[n],teksti,koko);
	n+=koko;

	msg[n]=';';
	n++;
	msg[n]='\0';
	n++;
	int len=n;

	if(joinsoketti!=NULL){
		int result=SDLNet_TCP_Send(joinsoketti,msg,len);
		if(result<len) {
			printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
		}
	}

}

void tekstiValmis(){
	//printf("lisätään rivi\n");
	if(hostpeli){
//		printf("host\n");
		char temprivi[511];
		memcpy(temprivi,sets->nick[0],10);
		int i;
		for(i=0;i<10;i++){
			if(temprivi[i]=='\0')temprivi[i]=' ';
		}
		temprivi[10]=':';
		memcpy(&temprivi[11],kirjotusbuffer,strlen((const char*)kirjotusbuffer));
		lisaaRivi(temprivi,strlen((const char*)temprivi));
		lahetaRiviKlienteille(kirjotusbuffer,strlen((const char*)kirjotusbuffer));
	}else {//joinpeli
		if(joinpeli){
			//printf("join\n");
		}
		lahetaRiviHostille(kirjotusbuffer,strlen((const char*)kirjotusbuffer));
	}
	tyhjennaKirjotusBufferi();
	vasenkirjain=0;
	kirjotettavakirjain=0;
}

void alustaChat(){
	vasenkirjain=0;
	kirjotettavakirjain=0;
	tyhjennaKirjotusBufferi();
	tyhjennaChatBufferi();
}

void tyhjennaKirjotusBufferi(){
	int i;
  vikakirjain=0;
	kirjotettavakirjain=0;
	vasenkirjain=0;
	for(i=0;i<MAXCHATKIRJOTUSMERKIT;i++){
		kirjotusbuffer[i]=0;
	}
}

void tyhjennaChatBufferi(){
	int i,j;
	for(i=0;i<MAXCHATRIVIT;i++){
		for(j=0;j<MAXCHATMERKIT;j++){
			chatbuffer[i][j]=0;
		}
	}
}

void tyhjennaChatRivi(int rivi){
	if(rivi<MAXCHATRIVIT){
		memset(&chatbuffer[rivi],0,sizeof(char)*MAXCHATMERKIT);
	}
}

void lisaaRivi(char *data, int pituus){
	printf("lisätään rivi %s,%i\n",data,pituus);
	int i, kohta=11;
	bool valmis=false;
	while(!valmis){
		for(i=MAXCHATRIVIT-1;i>0;i--){
			memcpy(&chatbuffer[i],&chatbuffer[i-1],sizeof(char)*MAXCHATMERKIT);
		}
		tyhjennaChatRivi(0);
		if(pituus-kohta<MAXCHATMERKIT-11){
			valmis=true;
			memcpy(&chatbuffer[0],data,11);
			memcpy(&chatbuffer[0][11],&data[kohta],sizeof(char)*pituus-kohta);
		}else {
			memcpy(&chatbuffer[0],data,11);
			memcpy(&chatbuffer[0][11],&data[kohta],sizeof(char)*(MAXCHATMERKIT-12));
			chatbuffer[0][MAXCHATMERKIT-1]='\0';
			kohta+=MAXCHATMERKIT-12;
		}
	}
}

char chatbuffer[MAXCHATRIVIT][MAXCHATMERKIT];
char kirjotusbuffer[MAXCHATKIRJOTUSMERKIT];
int vikakirjain, kirjotettavakirjain, vasenkirjain, ruudunleveys=85;
