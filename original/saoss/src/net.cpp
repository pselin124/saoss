#include "main.h"
#include "net.h"
#include "Sets.h"
#include "netgamesets.h"
#include "chat.h"

#ifdef WIN32
//#include <windows.h>
#endif

int timeoutaika=250;
int timeouttarkistettu=0;
int timeout[MAXNETPLAYERS];
int juuriluettu=0;

int soketteja=0;

TCPsocket joinsoketti=NULL;
SDLNet_SocketSet set2;
TCPsocket tcpsoketit[16];

int kuuntelusoketti=0;
int tila[16];
int pelaajiasoketista[MAXNETPLAYERS];
int pelaajasoketista[MAXNETPLAYERS];
int connectionstatus[MAXNETPLAYERS];

int mptiimi[MAXNETPLAYERS];
int mpstatus[MAXNETPLAYERS];
int hosteja=1;

int started2=0;
int joinaavia=1;
int playerslots=16;
int mpcredits=250;
int gamemode=DEATHMATCHRANDOM;
int luetaan=0;

bool gamequited;
bool alkujuttusaatu=false;
bool valmiinaalottamaan=false;
//bool pelaajialiikaaerror=false;

char playeruudestaan[MAXNETPLAYERS];

asteroidipox arajahdys;

void startMenu(){
	glDisable(GL_LIGHTING);
	glEnable(GL_BLEND);
	glBlendFunc(GL_ONE,GL_ONE_MINUS_SRC_COLOR);	
//	glDisable(GL_BLEND);
	glDisable(GL_DEPTH_TEST);
	glEnable(GL_TEXTURE_2D);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);			
	gluPerspective(45.0f,(GLfloat)screenwidth/(GLfloat)screenheight,1.0f,100.0f);
	glLoadIdentity();							
	glMatrixMode(GL_MODELVIEW);

	gluLookAt(0,0,15,
	0,0,0,
	0,1,0);

}
void stopMenu(){
	glEnable(GL_BLEND);
	SDL_GL_SwapBuffers();
}

//SDLNet_SocketSet set;

void paivitaPelaajat(){
	int i,k=0,j;
	int cpu=1,closed=0;
	for(i=0;i<playerslots;i++){
			if(tila[i]<=0) pelaajiasoketista[i]=0;
	}
	for(i=0;i<playerslots;i++)if(tila[i]>0){
		if(k<playerslots){
			for(j=0;j<pelaajiasoketista[i];){
				if(k<playerslots && (mpstatus[k]<1 || mpstatus[k]>2)){
					pelaajasoketista[k]=i;
					memcpy(&netnick[k],&tcpnick[i][j],10);
					mpstatus[k]=3;
					k++;
					j++;
				}else if(k<playerslots){
					if(mpstatus[k]==2){
						pelaajasoketista[k]=-2;
						sprintf(netnick[k],"cpu %i",cpu);
						cpu++;
					}
					else {
						pelaajasoketista[k]=-1;
						sprintf(netnick[k],"closed");
					}
					k++;
				}
				if(k>=playerslots)break;
			}
		}
	}
	for(i=k;i<playerslots;i++){
		if(mpstatus[i]==3) mpstatus[i]=0;
		if(mpstatus[i]==0) sprintf(netnick[i],"-");
		if(mpstatus[i]==2){
			sprintf(netnick[i],"cpu %i",cpu);
			cpu++;
		}
		if(mpstatus[i]==1) sprintf(netnick[i],"-");
	}
}

void addsocket(){
//	int i,j;
	IPaddress ip;
	if(SDLNet_ResolveHost(&ip,NULL,9999)==-1) {
		printf("SDLNet_ResolveHost: %s\n", SDLNet_GetError());
		exit(3);
	}
	tcpsoketit[0]=SDLNet_TCP_Open(&ip);
	if(!tcpsoketit[0]) {
		printf("SDLNet_TCP_Open: %s\n", SDLNet_GetError());
		exit(2);
	}
	tila[0]=1;

}

int addreadysocket(TCPsocket socket){
	int i,j=-1;
	for(i=1;i<16;i++){
		if(tcpsoketit[i]==NULL){
			tcpsoketit[i]=socket;
			tila[i]=1;
			timeout[i]=TIME_OUT_MAARA;
			int len,result;
			char *msg="you have joined the game;";
			len=strlen(msg)+1;
			if(tcpsoketit[i]!=NULL){
				result=SDLNet_TCP_Send(tcpsoketit[i],msg,len);
				if(result<len) {
					printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
					// It may be good to disconnect sock because it is likely invalid now.
				}
			}
			soketteja++;
			j=i;
			i=16;
		}
	}
	return j;
}

void lahetaAlkuRuutu(TCPsocket socket){
	int i;
	char viesti[MAXNETPLAYERS*13+50];
	sprintf(viesti,"tiimit");
	viesti[6]=(char)playerslots;
	int n=7;
	for(i=0;i<playerslots;i++){
		viesti[n]=(char)mptiimi[i];
		n++;
		viesti[n]=(char)mpstatus[i];
		n++;
		memcpy(&viesti[n],&netnick[i],11);
		n+=11;
	}
//	AlkuTiedot tiedot;
/*	tiedot.gamemode=gamemode;
	tiedot.mpcredits=mpcredits;
	tiedot.fps=sets->fps;
	memcpy(&viesti[n],&tiedot,sizeof(AlkuTiedot));
	n+=sizeof(AlkuTiedot);*/
	char tempgamemode=gamemode;
	memcpy(&viesti[n],&tempgamemode,sizeof(char));
	n+=sizeof(char);
	short tempmpcredits=mpcredits;
	memcpy(&viesti[n],&tempmpcredits,sizeof(short));
	n+=sizeof(short);
	float tempfps=calculatefps;
	memcpy(&viesti[n],&tempfps,sizeof(float));
	n+=sizeof(float);

	viesti[n]=';';
	n++;
	viesti[n]='\0';
	int len=n;
	int result=SDLNet_TCP_Send(socket, viesti,len);
	if(result<len) {
		printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
	}

}

void lahettele(){
	int i;
	int len,result;
	char *msg="pelaajia montako;";
	len=strlen(msg)+1;
	for(i=1;i<playerslots;i++){
		if(tcpsoketit[i]!=NULL){
			if(pelaajiasoketista[i]==0){
				result=SDLNet_TCP_Send(tcpsoketit[i],msg,len);
				if(result<len) {
					printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
				}
			}
		}
	}
}

void disconnect(int i){
	tila[i]=0;
	SDLNet_TCP_Close(tcpsoketit[i]);
	tcpsoketit[i]=NULL;
	pelaajiasoketista[i]=0;
	int j;
	if(hostpeli){
		for(j=0;j<players;j++){
			if(pelaajasoketista[j]==i){
				mpstatus[j]=2;
				player[j].ihmispelaaja=false;
			}
		}
	}
}

void kuuntele(){
	int numready=0,i,numero=-1;
	if(SDL_GetTicks() >= (unsigned int)(timeouttarkistettu+timeoutaika)){
		timeouttarkistettu=SDL_GetTicks();
		for(i=1;i<MAXNETPLAYERS;i++) {
			if(tila[i]>0){
				timeout[i]--;
				if(timeout[i]<1) disconnect(i);
				else{
					lahetaAlkuRuutu(tcpsoketit[i]);
				}
			}
		}
	}
	SDLNet_SocketSet set=SDLNet_AllocSocketSet(16);
	for(i=0;i<16;i++){
		if(tcpsoketit[i]!=NULL && tila[i]>0) int numused=SDLNet_TCP_AddSocket(set,tcpsoketit[i]);
	}
	numready=SDLNet_CheckSockets(set, 20);
	if(numready==-1) {
		printf("SDLNet_CheckSockets: %s\n", SDLNet_GetError());
		perror("SDLNet_CheckSockets");
	}
	else if(numready) {
		if(SDLNet_SocketReady(tcpsoketit[0])) {
			TCPsocket client=SDLNet_TCP_Accept(tcpsoketit[0]);
			soketteja++;
			numero=addreadysocket(client);
			printf("Tulipa yksi soketti taas lis‰tty‰\n");
			int numused=SDLNet_TCP_AddSocket(set,tcpsoketit[numero]);
		}
	}
/*	if(juuriluettu){
		juuriluettu=false;
	}*/
	if(numready) for(i=1;i<playerslots;i++){
		if(tcpsoketit[i]!=NULL && tila[i]>=0 && i!=numero){
//			printf("kuunnellaan sokettia %i\n",i);
			if(SDLNet_SocketReady(tcpsoketit[i])!=0){
				//printf("soketti %i sanoo jotain..\n",i);
				int result,j;
				char msg[MAXLEN];
				for(j=0;j<MAXLEN;j++) msg[j]=0;
				result=SDLNet_TCP_Recv(tcpsoketit[i],msg,MAXLEN);
				if(result<0) {
					printf("jotain hassua p‰‰si k‰ym‰‰n, tuli %i byte‰.\n",result);// An error may have occured, but sometimes you can just ignore it
					disconnect(i);
				}
				else if(result>0){
					hostNetKasittele(i,msg,result);
/*					if(compare(msg,"team",4)){
						char luku=msg[4];
						if(luku<playerslots && pelaajasoketista[luku]==i){
							mptiimi[luku]++;
							if(mptiimi[luku]>=MAXNETPLAYERS) mptiimi[luku]=0;
						}
					}
					else if(compare(msg,"x‰t",3)){
						lisaaRivi(&msg[3],strlen((const char*)&msg[3]-1));
						int j2;
						for(j2=1;j2<MAXNETPLAYERS;j2++){
							if(tcpsoketit[j2]!=NULL && tila[j2]>=0){
								int result=SDLNet_TCP_Send(tcpsoketit[j2], msg,strlen((const char*)msg));
								if(result<strlen((const char*)msg)) {
									printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
								}
							}

						}
					}
					else if(compare(msg,"pelaajia ",7)){
						char luku=msg[9];
						printf("saatu:pelaajia soketista %i %i kpl",i,luku);		
						int vapaita=0;
						for(j=0;j<playerslots;j++){
							if(mpstatus[j]==0) vapaita++;
						}
						if(vapaita<luku){
							char viesti[20];
							sprintf(viesti,"pelaajia liikaa!");
							int n=16;
							viesti[n]=';';
							n++;
							viesti[n]='\0';
							n++;
							int len=n;
							int result=SDLNet_TCP_Send(tcpsoketit[i], viesti,len);
							if(result<len) {
								printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
							}
							disconnect(i);
						}else{
							pelaajiasoketista[i]=luku;
							int nikkeja=luku;
							int index;
							int n=10;
							if(nikkeja<5) for(index=0;index<nikkeja;index++){
								int pituus=msg[n];
								n++;
								if(pituus<12){
									memcpy(&tcpnick[i][index],&msg[n],pituus);
								}
								n+=pituus;
							}
						}

					}else if(compare(msg,"alku huomattu",10)){
						if(tila[i]==1) tila[i]=2;
					}else if(compare(msg,"tiimit saatu",10)){
						if(tila[i]==2) tila[i]=3;
					}
					//printf("Kuuntelija, Received: \"%s\" from socket:%i\n",msg,i);
					timeout[i]=TIME_OUT_MAARA;
					//paivitaPelaajat();
					*/
				}
				else printf("nollaa byte‰ sataa\n");
				juuriluettu=1;
			}
		}
	}
	SDLNet_FreeSocketSet(set);
}

void hostNetKasittele(int i, char *msg, int result){
	int n;
	if(compare(msg,"team",4)){
		n=4;
		char luku=msg[4];
		if(luku<playerslots && pelaajasoketista[luku]==i){
			mptiimi[luku]++;
			if(mptiimi[luku]>=MAXNETPLAYERS) mptiimi[luku]=0;
		}
		n++;
	}
	else if(compare(msg,"x‰t",3)){
		lisaaRivi(&msg[3],strlen((const char*)&msg[3]-1));
		int j2;
		for(j2=1;j2<MAXNETPLAYERS;j2++){
			if(tcpsoketit[j2]!=NULL && tila[j2]>=0){
				int result=SDLNet_TCP_Send(tcpsoketit[j2], msg,strlen((const char*)msg));
				if((unsigned int)result<(unsigned int)strlen((const char*)msg)) {
					printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
				}
			}
		}
		n=strlen((const char*)&msg[0]);
	}
	else if(compare(msg,"pelaajia ",7)){
		char luku=msg[9];
		printf("saatu:pelaajia soketista %i %i kpl",i,luku);		
		int vapaita=0;
		int j;
		for(j=0;j<playerslots;j++){
			if(mpstatus[j]==0) vapaita++;
		}
		if(vapaita<luku){
			char viesti[20];
			sprintf(viesti,"pelaajia liikaa!");
			int n2=16;
			viesti[n2]=';';
			n2++;
			viesti[n2]='\0';
			n2++;
			int len=n2;
			int result=SDLNet_TCP_Send(tcpsoketit[i], viesti,len);
			if(result<len) {
				printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
			}
			disconnect(i);
			n=10;
		}else{
			pelaajiasoketista[i]=luku;
			int nikkeja=luku;
			int index;
			n=10;
			if(nikkeja<5) for(index=0;index<nikkeja;index++){
				int pituus=msg[n];
				n++;
				if(pituus<12){
					memcpy(&tcpnick[i][index],&msg[n],pituus);
				}
				n+=pituus;
			}
		}
	}else if(compare(msg,"alku huomattu",10)){
		if(tila[i]==1) tila[i]=2;
		n=13;
	}else if(compare(msg,"tiimit saatu",10)){
		if(tila[i]==2) tila[i]=3;
		n=13;
	}
	//printf("Kuuntelija, Received: \"%s\" from socket:%i\n",msg,i);
	timeout[i]=TIME_OUT_MAARA;
	//paivitaPelaajat();
	if(result>n+7){
		for(i=0;i<7;i++){
			if(msg[n]==';'){
				n++;
				if(!(msg[n]>='a' && msg[n]<='z'))n++;
				if(!(msg[n]>='a' && msg[n]<='z'))n++;
				if(!(msg[n]>='a' && msg[n]<='z'))n++;
				//printf("jatketaan viestin k‰sittely‰ kohdasta %s\n",&msg[n]);
				if(result-n>3){
					//packets--;
					hostNetKasittele(i, &msg[n], result-n);
					return;
				}
			}
			n++;
		}
	}
}


int compare(char *eka, char *toka, int maara){
	int i;
	for(i=0;i<maara;i++){
		if(eka[i]=='\0' && toka[i]=='\0') return 1;
		else if(eka[i]!=toka[i]) return 0;
	}
	return 1;
}
/*			if(client) {
				#define MAXLEN 1024
				int result;
				char msg[MAXLEN];
					result=SDLNet_TCP_Recv(client,msg,MAXLEN);
				if(result<=0) {
					// An error may have occured, but sometimes you can just ignore it
					// It may be good to disconnect sock because it is likely invalid now.
				}
				printf("Received: \"%s\"\n",msg);
				// play with the client.
				SDLNet_TCP_Close(client);
			}
			SDLNet_FreeSocketSet(set);
*/


void drawHostMPMenu(int selection, int rivi){
	startMenu();
	glPushMatrix();



		glColor4f(1.0f,1.0f,1.0f,1.0f);		
		glBindTexture(GL_TEXTURE_2D, texture[29]);				
	float tempkoko=3.0f/4.0f;
//		float tempkoko=(float)screenheight/(float)screenwidth;
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f, -1.0f*tempkoko, 13);
			glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f, -1.0f*tempkoko, 13);
			glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f,  1.0f*tempkoko, 13);
			glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f,  1.0f*tempkoko, 13);
		glEnd();

		glColor4f(1,1,1,1);
/*		glBindTexture(GL_TEXTURE_2D, texture[18]);				
		glPushMatrix();
			if(rivi==0){
				glTranslatef(-8.1,5.65-selection*0.6,0);
			}else if(rivi==1){
				glTranslatef( 1.0,5.65-selection*0.6,0);
			}else if(rivi==2){
				glTranslatef( 4.0,5.65-selection*0.6,0);
			}
			tempkoko=0.6;
			glBegin(GL_QUADS);
				glNormal3f(0,0,1.0f);
				glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*tempkoko, -1.0f*tempkoko, 0);
				glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*tempkoko, -1.0f*tempkoko, 0);
				glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*tempkoko,  1.0f*tempkoko, 0);
				glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*tempkoko,  1.0f*tempkoko, 0);
			glEnd();
		glPopMatrix();*/
		glColor4f(1,0,0,1);
		char *vekotin[]={	"start game",
						"settings",
						"chat",
						"exit"};

		int i;
		for(i=0;i<4;i++){
			gl_println( -33,  28-i*3, -60,vekotin[i]);
		}
		char luku[12];

		for(i=0;i<playerslots;i++){
			//sprintf(luku,"player %i",i+1);
			if(mpstatus[i]==0){
				glColor4f(0.2,1,0.2,1);
				gl_println(  8,  28-i*3, -60, "open");
			}
			else if(mpstatus[i]==1){
				glColor4f(1,0.2,0.2,1);
				gl_println(  8,  28-i*3, -60, "closed");
			}
			else if(mpstatus[i]==2){
				glColor4f(1,0.2,1,1);
				gl_println(  8,  28-i*3, -60, "cpu");
			}
			else if(mpstatus[i]==3){
				glColor4f(0.2,0.2,1,1);
				gl_println(  8,  28-i*3, -60, "human");
			}

			sprintf(luku,netnick[i]);
			gl_println( -14,  28-i*3, -60, luku);

			if(mpstatus[i]==2 || mpstatus[i]==3)  sprintf(luku,"team %i", mptiimi[i]+1);
			else sprintf(luku,"team %i", 0);

			gl_println( 23,  28-i*3, -60, luku);

		}
		glColor4f(1,0,0,1);
		gl_println( -37,  -25, -60,"game mode:",1.2);
		glColor4f(0,0,1,1);
		if(gamemode==DEATHMATCHRANDOM){
			gl_println( -13,-25, -60,"death match with random ships",1);
		}else if(gamemode==DOMINATION){
			gl_println( -13,-25, -60,"flower domination",1);
		}else if(gamemode==CTF){
			gl_println( -13,-25, -60,"capture the flower",1);
		}else if(gamemode==MUSHROOM){
			gl_println( -13,-25, -60,"eat the mushroom",1);
		}else{
			gl_println( -13,-25, -60,"death match",1);
		}



GLfloat specular[4] = {2, 2, 2, 1};
glMaterialfv(GL_FRONT, GL_SPECULAR, specular);
glMaterialf(GL_FRONT, GL_SHININESS, 120);	
glLightfv(GL_LIGHT0,GL_SPECULAR,specular);
GLfloat lightdiffuse[4]= {0.5, 0.5, 0.5, 0.5};
glLightfv(GL_LIGHT0,GL_DIFFUSE, lightdiffuse);
GLfloat lightposition[4] = {0, 0, 2, 0};
glLightfv(GL_LIGHT0,GL_POSITION,lightposition);

  glPushMatrix();
		glEnable(GL_LIGHTING);
		glDisable(GL_BLEND);
		glEnable(GL_DEPTH_TEST);
		glDisable(GL_TEXTURE_2D);
			if(rivi==0){
				glTranslatef(-7.7,5.62-selection*0.6,0);
				glScalef(0.25,0.3,0.3);
			}else if(rivi==1){
				glTranslatef( 0.3,5.62-selection*0.6,0);
				glScalef(0.3,0.3,0.3);
			}else if(rivi==2){
				glTranslatef( 3.3,5.62-selection*0.6,0);
				glScalef(0.27,0.3,0.3);
			}
		glRotatef(nuolirot,1,0,0);
		//glScalef(0.3,0.3,0.3);
		glCallList(NUOLI);
		nuolirot+=2.5;
		if(nuolirot>180)nuolirot-=360;
	glPopMatrix();


	glPopMatrix();
	stopMenu();
}

void drawMontakoHostia(int selection, int hosteja){
	startMenu();
	glPushMatrix();
		glColor4f(1,1,1,1);
		//glBindTexture(GL_TEXTURE_2D, texture[18]);				
	glColor4f(1.0f,1.0f,1.0f,1.0f);		
	glBindTexture(GL_TEXTURE_2D, texture[31]);	//27			
	float tempkoko=3.0f/4.0f;
//	float tempkoko=(float)screenheight/(float)screenwidth;
	//float tempkoko=0.6f;
	glPushMatrix();
		glTranslatef(0,0,15);
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f, -1.0f*tempkoko, -1.8);
			glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f, -1.0f*tempkoko, -1.8);
			glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f,  1.0f*tempkoko, -1.8);
			glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f,  1.0f*tempkoko, -1.8);
		glEnd();
		glBlendFunc(GL_ZERO,GL_ONE_MINUS_SRC_ALPHA);	
		glColor4f(0,0,0,0.6);
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			glTexCoord2f(0.0f, 0.0f); glVertex3f(-0.9f, -0.15f, -2);
			glTexCoord2f(1.0f, 0.0f); glVertex3f(-0.0f, -0.15f, -2);
			glTexCoord2f(1.0f, 1.0f); glVertex3f(-0.0f,  0.25f, -2);
			glTexCoord2f(0.0f, 1.0f); glVertex3f(-0.9f,  0.25f, -2);

		glEnd();
		glBlendFunc(GL_ONE,GL_ONE_MINUS_SRC_COLOR);	
	glPopMatrix();
/*		glPushMatrix();
			glTranslatef(-8.0f,1.6f-selection*0.7f,0);
			//if(selection==0) glTranslatef(-8.0f,1.60,0);
			//else if(selection==1) glTranslatef(-8.0f,0.90,0);
			float tempkoko=0.6;
			glBegin(GL_QUADS);
				glNormal3f(0,0,1.0f);
				glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*tempkoko, -1.0f*tempkoko, 0);
				glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*tempkoko, -1.0f*tempkoko, 0);
				glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*tempkoko,  1.0f*tempkoko, 0);
				glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*tempkoko,  1.0f*tempkoko, 0);
			glEnd();
		glPopMatrix();*/
		glColor4f(1,0,0,1);
		char maara[10];
		sprintf(maara,"%i",hosteja);
		char *vekotin[]={ "players from this machine:",
											"change nicknames",
											"next",
											"exit"};
					

		int i;
		for(i=0;i<4;i++){
			gl_println( -32,  8-3.5*i, -60,vekotin[i]);
		}
		//gl_println( -35,  8, -60,vekotin[0]);
		gl_println(  -3,  8, -60,maara);
		//gl_println( -35,  4.5, -60,vekotin[1]);

GLfloat specular[4] = {2, 2, 2, 1};
glMaterialfv(GL_FRONT, GL_SPECULAR, specular);
glMaterialf(GL_FRONT, GL_SHININESS, 120);	
glLightfv(GL_LIGHT0,GL_SPECULAR,specular);
GLfloat lightdiffuse[4]= {0.5, 0.5, 0.5, 0.5};
glLightfv(GL_LIGHT0,GL_DIFFUSE, lightdiffuse);
GLfloat lightposition[4] = {0, 0, 2, 0};
glLightfv(GL_LIGHT0,GL_POSITION,lightposition);

  glPushMatrix();
		glEnable(GL_LIGHTING);
		glDisable(GL_BLEND);
		glEnable(GL_DEPTH_TEST);
		glDisable(GL_TEXTURE_2D);
		glTranslatef(-7.5f,1.6f-selection*0.7f,0);
		glRotatef(nuolirot,1,0,0);
		glScalef(0.28,0.3,0.3);
		glCallList(NUOLI);
		nuolirot+=2.5;
		if(nuolirot>180)nuolirot-=360;
	glPopMatrix();


	glPopMatrix();
	stopMenu();
}

int readnick(char *nick){
	SDL_EnableUNICODE(1);
	bool menuready=false;
	entterpressed=false;
	luetaan=1;
	char teksti[48];
	int kohta=0;
/*	for(i=0;i<48;i++){
		iptext[i]='\0';
	}*/
	while(!menuready){
		processEvents();
		if(entterpressed || escpressed || luetaan==0){
			entterpressed=false;
			escpressed=false;
			menuready=true;
			luetaan=0;
		}
		if(luetaan>1){
			luetaan=1;
			bool control=false;
/*			if(lctrl || rctrl){
				control=true;
				printf("luettunappi i=%i, c=%c\n",luettunappi,luettunappi,modifier);
			}*/
			printf("luettunappi i=%i, c=%c\n",luettunappi,luettunappi,modifier);
			//if(luettunappi>='A' && luettunappi<='Z') luettunappi-='A'-'a';
			if( (sdlnappi == SDLK_v && modifier&KMOD_CTRL) ||
				(sdlnappi == SDLK_INSERT)){// && modifier&KMOD_SHIFT)){
#ifdef WIN32
				OpenClipboard(NULL);
				HANDLE h = GetClipboardData(CF_TEXT);
				GlobalLock(h);
				int koko=39-kohta;
				strncpy(&nick[kohta],(const char*)h,koko);
				//printf("moi, koko=%i,h=%s,teksti=%s\n",koko,h,nick);
				int tempkoko=strlen((const char*)h);
				if(tempkoko+kohta>38) kohta=38;
				else kohta+=tempkoko;
				GlobalUnlock(h);
				CloseClipboard();
#endif
			}
			else if(luettunappi==8){
				if(kohta>0){
					kohta--;
					nick[kohta]='\0';
				}
			}
			else if((luettunappi>='a' && luettunappi<='z') 
					|| (luettunappi>='A' && luettunappi<='Z')
					|| luettunappi=='.' 
					|| luettunappi=='‰' || luettunappi=='ƒ'
					|| luettunappi=='ˆ' || luettunappi=='÷'
					|| (luettunappi>='0' && luettunappi<='9')
					|| luettunappi=='-'
					|| luettunappi==' '){
				nick[kohta]=luettunappi;
				nick[kohta+1]='\0';
				if(kohta<8) kohta++;
			}
		}
		do{
			currenttime=SDL_GetTicks()-starttime;
			framecounter=calculatefps*currenttime/1000.0;
			if(oldframecounter==framecounter) SDL_Delay(1);
		}	while (oldframecounter==framecounter);
		oldframecounter=framecounter;
		startMenu();
			char kirjaimet[16];
			sprintf(kirjaimet,"enter nickname:");
			sprintf(teksti,"%s",nick);
			glColor4f(1,0,0,1);
			gl_println(-10,  10,-60,kirjaimet);
			gl_println(-38,  6,-60,teksti);
		stopMenu();
		updateAudio();
	}
	return 0;
}

void drawNikinVaihtoMenu(int selection){
	startMenu();
	glPushMatrix();
		glColor4f(1,1,1,1);
		glBindTexture(GL_TEXTURE_2D, texture[18]);				
		glPushMatrix();
			glTranslatef(-8.0f,1.6f-selection*0.7f,0);
			//if(selection==0) glTranslatef(-8.0f,1.60,0);
			//else if(selection==1) glTranslatef(-8.0f,0.90,0);
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
		char maara[10];
		sprintf(maara,"%i",hosteja);
		char *vekotin[]={ "change nick 1:",
											"change nick 2:",
											"change nick 3:",
											"change nick 4:",
											"exit"};
					

		int i;
		for(i=0;i<5;i++){
			gl_println( -35,  8-3.5*i, -60,vekotin[i]);
		}
		for(i=0;i<4;i++){
			gl_println( -15,  8-3.5*i, -60,sets->nick[i]);
		}

	glPopMatrix();
	stopMenu();
}

void nikinVaihtoMenu(){
	bool menuready=false , connect=false , exitmenu=false;
	entterpressed=false;
	int i=0;
	int pelaajia=1;
	player[i].pfire=false;
	player[i].palas=false;
	player[i].pright=false;
	player[i].pleft=false;
	int selectionmax=4;
	int selection=0;
	bool quit=false;
	while(!quit){
		while(!menuready){
			processEvents();
			if(entterpressed){
				entterpressed=false;
				menuready=true;
			}else if(player[0].pleft){
				menuready=true;
			}else if(player[0].pright){
				menuready=true;
			}else if(player[0].pylos){
				if(selection>0) selection--;
				else selection=selectionmax;
				player[0].pylos=false;
			}else if(player[0].palas){
				if(selection<selectionmax) selection++;
				else selection=0;
				player[0].palas=false;
			}else if(escpressed){
				selection=selectionmax;
				menuready=true;
				escpressed=false;
			}
			do{
				currenttime=SDL_GetTicks()-starttime;
				framecounter=calculatefps*currenttime/1000.0;
				if(oldframecounter==framecounter) SDL_Delay(5);
			}	while (oldframecounter==framecounter);
			oldframecounter=framecounter;
			drawNikinVaihtoMenu(selection);
			updateAudio();
		}
		menuready=false;
		if(selection==selectionmax){
			sets->saveFile();
			quit=true;
		}else if(selection>=0){
			readnick(sets->nick[selection]);
		}
		player[0].pleft=false;
		player[0].pright=false;
	}
}

int montakoHostia(){
	bool menuready=false , connect=false , exitmenu=false;
	entterpressed=false;
	int i=0;
	int pelaajia=1;
	player[i].pfire=false;
	player[i].palas=false;
	player[i].pright=false;
	player[i].pleft=false;
	int selectionmax=3;
	int selection=2;
	while(!connect && !exitmenu){
		while(!menuready){
			processEvents();
			if(entterpressed){
				entterpressed=false;
				menuready=true;
			}else if(player[0].pleft){
				menuready=true;
			}else if(player[0].pright){
				menuready=true;
			}else if(player[0].pylos){
				if(selection>0) selection--;
				else selection=selectionmax;
				player[0].pylos=false;
			}else if(player[0].palas){
				if(selection<selectionmax) selection++;
				else selection=0;
				player[0].palas=false;
			}else if(escpressed){
				selection=selectionmax;
				menuready=true;
				escpressed=false;
			}
			do{
				currenttime=SDL_GetTicks()-starttime;
				framecounter=calculatefps*currenttime/1000.0;
				if(oldframecounter==framecounter) SDL_Delay(5);
			}	while (oldframecounter==framecounter);
			oldframecounter=framecounter;
//			kuuntele(set);
			drawMontakoHostia(selection,pelaajia);
			updateAudio();
		}
		menuready=false;
		if(selection==2){
			return pelaajia;
		}else if(selection==0){
			if(player[0].pleft){
				if(pelaajia>0) pelaajia--;
				else pelaajia=4;
			}else{
				if(pelaajia<4)pelaajia++;
				else pelaajia=0;
			}
		}else if(selection==1){
			nikinVaihtoMenu();
		}else if(selection==selectionmax){
			return -1;
		}
		player[0].pleft=false;
		player[0].pright=false;
	}
	return 1;
}

int hostMenu(){
	int i;
	for(i=0;i<MAXNETPLAYERS;i++){
		pelaajiasoketista[i]=0;
	}
	hosteja=montakoHostia();
	hostpeli=true;
	if(hosteja<0)return 0;
	pelaajiasoketista[0]=hosteja;
	bool menuready=false , connect=false , exitmenu=false;
	entterpressed=false; spacepressed=false;
	for(i=0;i<hosteja;i++){
		mptiimi[i]=0;
		mpstatus[i]=3;
		sprintf(netnick[i],"%s",sets->nick[i]);
		sprintf(tcpnick[0][i],"%s",sets->nick[i]);
	}
	for(i=hosteja;i<MAXNETPLAYERS;i++){
		mptiimi[i]=1;
		mpstatus[i]=0;
	}
	alustaChat();
	player[i].pfire=false;
	player[i].palas=false;
	player[i].pright=false;
	player[i].pleft=false;
	int selectionmax=3;
	int selection=0;
	int selectionrivi=0;
	int maxselectionrivi=2;
	while(!connect && !exitmenu){
		while(!menuready){
			processEvents();
			if(entterpressed || spacepressed){
				entterpressed=false;
				spacepressed=false;
				menuready=true;
			}else if(player[0].pleft){
				if(selectionrivi>0){
					selectionrivi--;
					if(selectionrivi==0){
						selectionmax=3;
						if(selection>selectionmax)selection=selectionmax;
					}

				}else{
					selectionmax=playerslots-1;
					selectionrivi=maxselectionrivi;
				}
				player[0].pleft=false;
			}else if(player[0].pright){
				if(selectionrivi<maxselectionrivi){
					selectionmax=playerslots-1;
					selectionrivi++;
				}else{
					selectionmax=3;
					if(selection>selectionmax)selection=selectionmax;
					selectionrivi=0;
				}
				player[0].pright=false;
			}else if(player[0].pylos){
				if(selection>0) selection--;
				else selection=selectionmax;
				player[0].pylos=false;
			}else if(player[0].palas){
				if(selection<selectionmax) selection++;
				else selection=0;
				player[0].palas=false;
			}else if(escpressed){
				selection=selectionmax;
				selectionrivi=0;
				menuready=true;
				escpressed=false;
			}
			do{
				currenttime=SDL_GetTicks()-starttime;
				framecounter=calculatefps*currenttime/1000.0;
				if(oldframecounter==framecounter) SDL_Delay(5);
			}	while (oldframecounter==framecounter);
			oldframecounter=framecounter;
			paivitaPelaajat();
			kuuntele();
			lahettele();
			drawHostMPMenu(selection, selectionrivi);
			updateAudio();
		}
		menuready=false;
		entterpressed=false;
		spacepressed=false;
		if(selection==selectionmax && selectionrivi==0){
			hostQuit();
			return 0;
		}else if(selection==0 && selectionrivi==0){
			hostStart();
			return 0;
		}else if(selection==2 && selectionrivi==0){
			chatScreen();
			if(hostpeli) printf("hostpeli1\n");

		}else if(selection==1 && selectionrivi==0){
			hostSettingsMenu();
		}else if(selectionrivi==1){
//			printf("vaihtaa yritettiin, mpstatus[sel]=%i, sel=%i\n",mpstatus[selection],selection);
			if(selection<hosteja){
			}else if(mpstatus[selection]==3){
			}else{
				if(mpstatus[selection]<2) mpstatus[selection]++;
				else mpstatus[selection]=0;
//				printf("vaihdettiin mp[sel]=%i\n",mpstatus[selection]);
			}
		}else if(selectionrivi==2){
			if(mptiimi[selection]<MAXNETPLAYERS-1) mptiimi[selection]++;
			else mptiimi[selection]=0;
		}
	}
	return 0;
}

void hostStart(){
	int i;
	int len,result;
	char *msg="game started;";
	len=strlen(msg)+1;
	int suoritettu=0    ;
	while(!suoritettu){
		for(i=1;i<playerslots;i++){
			if(tcpsoketit[i]!=NULL){
				result=SDLNet_TCP_Send(tcpsoketit[i],msg,len);
				if(result<len) {
					printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
					// It may be good to disconnect sock because it is likely invalid now.
				}
			}
		}
		SDL_Delay(10);
		kuuntele();
		suoritettu=1   ;
		for(i=1;i<playerslots;i++){
			if(tila[i]==1) suoritettu=false;
		}
	}
	suoritettu=0    ;
	while(!suoritettu){
		for(i=1;i<playerslots;i++){
			if(tcpsoketit[i]!=NULL){
				lahetaAlkuRuutu(tcpsoketit[i]);
			}
		}
		SDL_Delay(10);
		kuuntele();
		suoritettu=1   ;
		for(i=1;i<playerslots;i++){
			if(tila[i]==2 || tila[i]==1) suoritettu=false;
			//printf("tila[%i]=%i",i,tila[i]);
		}

	}

	char *msg2="kaikki kunnossa;";
	len=strlen(msg2)+1;
	int j;
	for(j=0;j<1;j++){
		for(i=1;i<playerslots;i++){
			if(tcpsoketit[i]!=NULL && tila[i]>0){
				result=SDLNet_TCP_Send(tcpsoketit[i],msg2,len);
				if(result<len) {
					printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
				}else{
					printf("sanottiin %i:lle:\"%s\"",i,msg2);
				}
			}
		}
		SDL_Delay(20);
	}
	players=0;
	for(i=0;i<playerslots;i++){
		if(mpstatus[i]>1){
			if(players!=i) memcpy(&netnick[players],&netnick[i],11);
			players++;
		}
	}
	delete[] player;
	player=new Player[players];
	for(i=0;i<players;i++){
		double temp3=random(1000);
		player[i].asetaAlus(alus[1]);
		player[i].uudestaan(alus[(int)(temp3/1000.0f*(double)aluksia)]);
		player[i].kuolema=0;
		player[i].numero=i;
		player[i].pylos=false;
		player[i].pfire=false;
		player[i].pease=false;
		player[i].palas=false;
		player[i].pleft=false;
		player[i].pright=false;
		player[i].pfire=false;
		player[i].pisteet=fullcredits;
		player[i].tiimi=mptiimi[i];
		if(mpstatus[i]==2)player[i].ihmispelaaja=false;
		else player[i].ihmispelaaja=true;
	}

	for(i=0;i<players;i++){
		playeruudestaan[i]=0;
	}
	for(i=0;i<asteroideja;i++){
		asteroidiuudestaan[i]=true;
	}
	netHostPeli();
}

void hostQuit(){
	int i;
	int len,result;
	char *msg="quiting..;";
	len=strlen(msg)+1;
	for(i=1;i<playerslots;i++){
		if(tcpsoketit[i]!=NULL){
			result=SDLNet_TCP_Send(tcpsoketit[i],msg,len);
			if(result<len) {
				printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
				// It may be good to disconnect sock because it is likely invalid now.
			}
		}
	}
}


void drawHostSettingsMenu(int selection){
	startMenu();
	glPushMatrix();
		glColor4f(1,1,1,1);

glColor4f(1.0f,1.0f,1.0f,1.0f);		
	glBindTexture(GL_TEXTURE_2D, texture[31]);	//27			
	float tempkoko=3.0f/4.0f;
//	float tempkoko=(float)screenheight/(float)screenwidth;
	//float tempkoko=0.6f;
	glPushMatrix();
		glTranslatef(0,0,15);
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f, -1.0f*tempkoko, -1.8);
			glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f, -1.0f*tempkoko, -1.8);
			glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f,  1.0f*tempkoko, -1.8);
			glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f,  1.0f*tempkoko, -1.8);
		glEnd();
		glBlendFunc(GL_ZERO,GL_ONE_MINUS_SRC_ALPHA);	
		glColor4f(0,0,0,0.6);
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			glTexCoord2f(0.0f, 0.0f); glVertex3f(-0.9f, -0.22f, -2);
			glTexCoord2f(1.0f, 0.0f); glVertex3f( 0.4f, -0.22f, -2);
			glTexCoord2f(1.0f, 1.0f); glVertex3f( 0.4f,  0.25f, -2);
			glTexCoord2f(0.0f, 1.0f); glVertex3f(-0.9f,  0.25f, -2);

		glEnd();
		glBlendFunc(GL_ONE,GL_ONE_MINUS_SRC_COLOR);	
	glPopMatrix();

/*		glBindTexture(GL_TEXTURE_2D, texture[18]);				
		glPushMatrix();
			if(selection==0) glTranslatef(-8.0f,1.60,0);
			else if(selection==1) glTranslatef(-8.0f,0.90,0);
			else glTranslatef(-8.0f, 0.20-0.7*(selection-2),0);
			float tempkoko=0.6;
			glBegin(GL_QUADS);
				glNormal3f(0,0,1.0f);
				glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*tempkoko, -1.0f*tempkoko, 0);
				glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*tempkoko, -1.0f*tempkoko, 0);
				glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*tempkoko,  1.0f*tempkoko, 0);
				glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*tempkoko,  1.0f*tempkoko, 0);
			glEnd();
		glPopMatrix();*/
		glColor4f(1,0,0,1);
		char maara[10],maara2[10];
		sprintf(maara,"%i",playerslots);
		sprintf(maara2,"%i",mpcredits);
		char *vekotin[]={"maximum players:",
						"credits/limit:",//,0=unlimited:",
						"visual & audio settings",
						"game mode:",
						"exit menu"};

		glColor4f(1,0,0,1);
		gl_println( -32,  8, -60,vekotin[0]);
		gl_println( -32,  4.5, -60,vekotin[1]);
		gl_println( -32,  1, -60,vekotin[2]);
		gl_println( -32,-2.5, -60,vekotin[3]);
		gl_println( -32,-6.0, -60,vekotin[4]);

		glColor4f(0.2,0.2,1,1);
		gl_println(  -13,  8, -60,maara);
		gl_println( -18,  4.5, -60,maara2);
		if(gamemode==DEATHMATCHRANDOM){
			gl_println( -20,-2.5, -60,"death match with random ships",1);
		}else if(gamemode==DOMINATION){
			gl_println( -20,-2.5, -60,"flower domination");
		}else if(gamemode==CTF){
			gl_println( -20,-2.5, -60,"capture the flower");
		}else if(gamemode==MUSHROOM){
			gl_println( -20,-2.5, -60,"eat the mushroom");
		}else{
			gl_println( -20,-2.5, -60,"death match");
		}

GLfloat specular[4] = {2, 2, 2, 1};
glMaterialfv(GL_FRONT, GL_SPECULAR, specular);
glMaterialf(GL_FRONT, GL_SHININESS, 120);	
glLightfv(GL_LIGHT0,GL_SPECULAR,specular);
GLfloat lightdiffuse[4]= {0.5, 0.5, 0.5, 0.5};
glLightfv(GL_LIGHT0,GL_DIFFUSE, lightdiffuse);
GLfloat lightposition[4] = {0, 0, 2, 0};
glLightfv(GL_LIGHT0,GL_POSITION,lightposition);

  glPushMatrix();
		glEnable(GL_LIGHTING);
		glDisable(GL_BLEND);
		glEnable(GL_DEPTH_TEST);
		glDisable(GL_TEXTURE_2D);
			if(selection==0) glTranslatef(-7.5f,1.60,0);
			else if(selection==1) glTranslatef(-7.5f,0.90,0);
			else glTranslatef(-7.5f, 0.20-0.7*(selection-2),0);
		glRotatef(nuolirot,1,0,0);
		glScalef(0.28,0.3,0.3);
		glCallList(NUOLI);
		nuolirot+=2.5;
		if(nuolirot>180)nuolirot-=360;
	glPopMatrix();

	glPopMatrix();
	stopMenu();
}


void hostSettingsMenu(){
	bool menuready=false , connect=false , exitmenu=false;
	entterpressed=false;
	int i=0;
	player[i].pfire=false;
	player[i].palas=false;
	player[i].pright=false;
	player[i].pleft=false;
	int selectionmax=4;
	int selection=0;
	while(!connect && !exitmenu){
		while(!menuready){
			processEvents();
			if(entterpressed){
				entterpressed=false;
				menuready=true;
			}else if(player[0].pleft){
				menuready=true;
			}else if(player[0].pright){
				menuready=true;
			}else if(player[0].pylos){
				if(selection>0) selection--;
				else selection=selectionmax;
				player[0].pylos=false;
			}else if(player[0].palas){
				if(selection<selectionmax) selection++;
				else selection=0;
				player[0].palas=false;
			}else if(escpressed){
				selection=selectionmax;
				menuready=true;
				escpressed=false;
			}
			do{
				currenttime=SDL_GetTicks()-starttime;
				framecounter=calculatefps*currenttime/1000.0;
				if(oldframecounter==framecounter) SDL_Delay(5);
			}	while (oldframecounter==framecounter);
			oldframecounter=framecounter;
			kuuntele();
			lahettele();
			drawHostSettingsMenu(selection);
			updateAudio();
		}
		menuready=false;
		if(selection==selectionmax){
			exitmenu=1;
		}else if(selection==0){
			if(player[0].pleft==true){
				if(playerslots>2) playerslots--;
				else playerslots=MAXNETPLAYERS;

			}else{
				if(playerslots<MAXNETPLAYERS) playerslots++;
				else playerslots=2;
			}
			player[0].pright=false;
			player[0].pleft=false;
					
		}else if(selection==1){
			if(player[0].pright){
				if(mpcredits<1000) mpcredits+=25;
				else if(mpcredits<MAX_MP_CREDITS) mpcredits+=100;
				else mpcredits=25;
				if(mpcredits>MAX_MP_CREDITS) mpcredits=MAX_MP_CREDITS;
			}
			if(player[0].pleft==true){
				if(mpcredits>1000) mpcredits-=100;
				else if(mpcredits>25) mpcredits-=25;
				else mpcredits=MAX_MP_CREDITS;
				if(mpcredits<25) mpcredits=25;
			}
			player[0].pright=false;
			player[0].pleft=false;
		}else if(selection==2){
			netGameSets(false);
		}else if(selection==3){
			if(player[0].pleft){
				if(gamemode>0) gamemode--;
				else gamemode=MAXGAMEMODE;
			}else{
				if(gamemode<MAXGAMEMODE)gamemode++;
				else gamemode=0;
			}
		}
		player[0].pleft=false;
		player[0].pright=false;
	}
}

void hostMultiplayer(){
	int i;
	for (i=0;i<16;i++) tcpsoketit[i]=NULL;
	for (i=0;i<16;i++) tila[i]=0;
//	IPaddress ip;
//	TCPsocket tcpsock, client;
	//SDLNet_SocketSet set;

//	set=SDLNet_AllocSocketSet(16);
/*	if(!set) {
		printf("SDLNet_AllocSocketSet: %s\n", SDLNet_GetError());
		exit(1); //most of the time this is a major error, but do what you want.
	}*/
	addsocket();
	
	int laskuri=0;
	int temp=hostMenu();
/*	while(soketteja<2 && laskuri<5) {
		kuuntele(set);
		laskuri++;
	}
*/
	for(i=0;i<16;i++){
		if(tcpsoketit[i]!=NULL) SDLNet_TCP_Close(tcpsoketit[i]);
		tcpsoketit[i]=NULL;
	}
	//SDLNet_FreeSocketSet(set);
}

void dispCustomError(char *error){
	bool menuready=false;
	entterpressed=false;
	while(!menuready){
		processEvents();
		if(entterpressed){
			entterpressed=false;
			menuready=true;
		}
		do{
			currenttime=SDL_GetTicks()-starttime;
			framecounter=calculatefps*currenttime/1000.0;
			if(oldframecounter==framecounter) SDL_Delay(5);
		}	while (oldframecounter==framecounter);
		oldframecounter=framecounter;
		startMenu();
			char *vekotin[]={"press enter to continue.."};
			char errori[100];
			sprintf(errori,"%s", error);
			glColor4f(1,0,0,1);
			gl_println(-30,  12,-60,errori);
			gl_println(-30,  8, -60,vekotin[0]);
		stopMenu();
		updateAudio();
	}

	entterpressed=false;

}
void dispErrorFullScreen(){
	bool menuready=false;
	entterpressed=false;
	while(!menuready){
		processEvents();
		if(entterpressed){
			entterpressed=false;
			menuready=true;
		}
		do{
			currenttime=SDL_GetTicks()-starttime;
			framecounter=calculatefps*currenttime/1000.0;
			if(oldframecounter==framecounter) SDL_Delay(5);
		}	while (oldframecounter==framecounter);
		oldframecounter=framecounter;
		startMenu();
			char *vekotin[]={"press enter to continue.."};
			char errori[100];
			sprintf(errori,"%s", SDLNet_GetError());
			glColor4f(1,0,0,1);
			gl_println(-30,  12,-60,errori);
			gl_println(-30,  8, -60,vekotin[0]);
		stopMenu();
		updateAudio();
	}
	entterpressed=false;
}

void readHostFile(char *iptext){
	int i,size=0;
	FILE *ini;
	int errorlevel=0;
	for(i=0;i<48;i++){
		iptext[i]='\0';
	}
	if ((ini=fopen("lasthost.txt", "rb"))==NULL){
		printf("lashost.txt not found.\n");
		errorlevel=1;
	}
	if(!errorlevel){
		fseek(ini, 0, SEEK_END);
		size = ftell(ini);
		fseek(ini, 0, SEEK_SET);
		if(size>40)size=40;
		fread(iptext, size, 1, ini);
		size++;
		fclose(ini);
	}

/*	for(i=0;i<size-1;i++){
		if(		!(iptext[i]>='a' && iptext[i]>='Z') 
			&&	!(iptext[i]>='A' && iptext[i]>='Z') 
			&&	!(iptext[i]>='0' && iptext[i]>='9') 
			&&  !(iptext[i]=='.'))
			iptext[i]='\0';
	}*/
	for(i=size-1;i<48;i++){
		iptext[i]='\0';
	}
	
}

void writeHostFile(char *iptext){
	int i, size=0;
	FILE *ini;
	int errorlevel=0;
	for(i=0;i<90;i++){
		if(iptext[i]=='\0'){
			size=i;
			i=90;
		}
	}
	if ((ini=fopen("lasthost.txt", "wb"))==NULL){
		printf("cannot open lashost.txt for writing.\n");
		errorlevel=1;
	}
	if(!errorlevel){
		fseek(ini,0,0);
		if(size>64)size=64;
		fwrite(iptext,size,1,ini);
		fclose(ini);
	}
}

int readip(char *iptext){
	SDL_EnableUNICODE(1);
	bool menuready=false;
	entterpressed=false;
	luetaan=1;
	char teksti[100];
	int kohta=0;
/*	for(i=0;i<48;i++){
		iptext[i]='\0';
	}*/
	while(!menuready){
		processEvents();
		if(entterpressed || escpressed || luetaan==0){
			entterpressed=false;
			escpressed=false;
			menuready=true;
			luetaan=0;
		}
		if(luetaan>1){
			luetaan=1;
			bool control=false;
/*			if(lctrl || rctrl){
				control=true;
				printf("luettunappi i=%i, c=%c\n",luettunappi,luettunappi,modifier);
			}*/
			printf("luettunappi i=%i, c=%c\n",luettunappi,luettunappi,modifier);
			if(luettunappi>='A' && luettunappi<='Z') luettunappi-='A'-'a';
			if( (sdlnappi == SDLK_v && modifier&KMOD_CTRL) ||
				(sdlnappi == SDLK_INSERT)){// && modifier&KMOD_SHIFT)){
#ifdef WIN32
				OpenClipboard(NULL);
				HANDLE h = GetClipboardData(CF_TEXT);
				GlobalLock(h);
				int koko=63-kohta;
				strncpy(&iptext[kohta],(const char*)h,koko);
				printf("moi, koko=%i,h=%s,teksti=%s\n",koko,h,iptext);
				int tempkoko=strlen((const char*)h);
				if(tempkoko+kohta>63) kohta=63;
				else kohta+=tempkoko;
				GlobalUnlock(h);
				CloseClipboard();
#endif
			}
			else if(luettunappi==8){
				if(kohta>0){
					kohta--;
					iptext[kohta]='\0';
				}
			}
			else if((luettunappi>='a' && luettunappi<='z') 
					|| luettunappi=='.' 
					|| (luettunappi>='0' && luettunappi<='9')
					|| luettunappi=='-'){
				iptext[kohta]=luettunappi;
				iptext[kohta+1]='\0';
				if(kohta<62) kohta++;
			}
		}
		do{
			currenttime=SDL_GetTicks()-starttime;
			framecounter=calculatefps*currenttime/1000.0;
			if(oldframecounter==framecounter) SDL_Delay(1);
		}	while (oldframecounter==framecounter);
		oldframecounter=framecounter;
		startMenu();
			char kirjaimet[16];
			sprintf(kirjaimet,"enter ip/host:");
			sprintf(teksti,"%s",iptext);
			glColor4f(1,0,0,1);
			gl_println(-10,  10,-60,kirjaimet);
			gl_println(-38,  6,-60,teksti);
		stopMenu();
		updateAudio();
	}
	return 0;
}

void drawJoinMPMenu(int selection, char *iptext){
	startMenu();
	glPushMatrix();
		glColor4f(1,1,1,1);
/*		glBindTexture(GL_TEXTURE_2D, texture[18]);				
		glPushMatrix();
			if(selection==0) glTranslatef(-8.0f,1.60,0);
			else glTranslatef(-8.0f, 1.00-0.6*selection,0);
			float tempkoko=0.6;
			glBegin(GL_QUADS);
				glNormal3f(0,0,1.0f);
				glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*tempkoko, -1.0f*tempkoko, 0);
				glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*tempkoko, -1.0f*tempkoko, 0);
				glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*tempkoko,  1.0f*tempkoko, 0);
				glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*tempkoko,  1.0f*tempkoko, 0);
			glEnd();
		glPopMatrix();*/
	glBindTexture(GL_TEXTURE_2D, texture[31]);	//27			
	float tempkoko=(float)screenheight/(float)screenwidth;
	//float tempkoko=0.6f;
	glPushMatrix();
		glTranslatef(0,0,15);
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f, -1.0f*tempkoko, -1.8);
			glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f, -1.0f*tempkoko, -1.8);
			glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f,  1.0f*tempkoko, -1.8);
			glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f,  1.0f*tempkoko, -1.8);
		glEnd();
		glBlendFunc(GL_ZERO,GL_ONE_MINUS_SRC_ALPHA);	
		glColor4f(0,0,0,0.6);
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			glTexCoord2f(0.0f, 0.0f); glVertex3f(-0.9f, -0.33f, -2);
			glTexCoord2f(1.0f, 0.0f); glVertex3f( 0.1f, -0.33f, -2);
			glTexCoord2f(1.0f, 1.0f); glVertex3f( 0.1f,  0.27f, -2);
			glTexCoord2f(0.0f, 1.0f); glVertex3f(-0.9f,  0.27f, -2);

		glEnd();
		glBlendFunc(GL_ONE,GL_ONE_MINUS_SRC_COLOR);	
	glPopMatrix();
		glColor4f(1,0,0,1);
		char maara[10];
		sprintf(maara,"%i",joinaavia);
		char *vekotin[]={	"change ip",
						"now:",
						"players from this machine:",
						"audio & visual settings",
						"change nicknames",
						"connect",
						"exit"};

		gl_println( -30,  8, -60,vekotin[0]);
		gl_println( -37,  6.5, -80,vekotin[1]);
		gl_println( -27,  6.5, -80,iptext);
		gl_println( -30,  2, -60,vekotin[2]);
		gl_println(  -2,  2, -60,maara);
		gl_println( -30, -1, -60,vekotin[3]);
		gl_println( -30, -4, -60,vekotin[4]);
		gl_println( -30, -7, -60,vekotin[5]);
		gl_println( -30,-10, -60,vekotin[6]);

GLfloat specular[4] = {2, 2, 2, 1};
glMaterialfv(GL_FRONT, GL_SPECULAR, specular);
glMaterialf(GL_FRONT, GL_SHININESS, 120);	
glLightfv(GL_LIGHT0,GL_SPECULAR,specular);
GLfloat lightdiffuse[4]= {0.5, 0.5, 0.5, 0.5};
glLightfv(GL_LIGHT0,GL_DIFFUSE, lightdiffuse);
GLfloat lightposition[4] = {0, 0, 2, 0};
glLightfv(GL_LIGHT0,GL_POSITION,lightposition);

  glPushMatrix();
		glEnable(GL_LIGHTING);
		glDisable(GL_BLEND);
		glEnable(GL_DEPTH_TEST);
		glDisable(GL_TEXTURE_2D);
		if(selection==0) glTranslatef(-7.2f,1.60,0);
		else glTranslatef(-7.2f, 1.00-0.6*selection,0);
		glRotatef(nuolirot,1,0,0);
		glScalef(0.28,0.3,0.3);
		glCallList(NUOLI);
		nuolirot+=2.5;
		if(nuolirot>180)nuolirot-=360;
	glPopMatrix();

	glPopMatrix();
	stopMenu();
}

int joinMenu(char *iptext){
	bool menuready=false , connect=false , exitmenu=false;
	entterpressed=false;
	int i=0;
	player[i].pfire=false;
	player[i].palas=false;
	player[i].pright=false;
	player[i].pleft=false;
	int selectionmax=5;
	int selection=4;
	readHostFile(iptext);

	while(!connect && !exitmenu){
		while(!menuready){
			processEvents();
			if(entterpressed){
				entterpressed=false;
				menuready=true;
			}else if(player[0].pleft){
				menuready=true;
			}else if(player[0].pright){
				menuready=true;
			}else if(player[0].pylos){
				if(selection>0) selection--;
				else selection=selectionmax;
				player[0].pylos=false;
			}else if(player[0].palas){
				if(selection<selectionmax) selection++;
				else selection=0;
				player[0].palas=false;
			}else if(escpressed){
				selection=selectionmax;
				menuready=true;
				escpressed=false;
			}
			do{
				currenttime=SDL_GetTicks()-starttime;
				framecounter=calculatefps*currenttime/1000.0;
				if(oldframecounter==framecounter) SDL_Delay(5);
			}	while (oldframecounter==framecounter);
			oldframecounter=framecounter;
			drawJoinMPMenu(selection, iptext);
			updateAudio();
		}
		menuready=false;
		if(selection==selectionmax){
			return 0;
		}else if(selection==0){
			int juttu=readip(iptext);
			writeHostFile(iptext);
		}else if(selection==1){
			if(player[0].pleft){
				if(joinaavia>1) joinaavia--;
				else joinaavia=4;
			}else{
				if(joinaavia<4)joinaavia++;
				else joinaavia=1;
			}
		}else if(selection==3){
			nikinVaihtoMenu();
		}
		else if(selection==4){
			return 1;
		}else if(selection==2){
			netGameSets(false);
		}
		player[0].pleft=false;
		player[0].pright=false;
	}
	return 0;
}


void joinMultiplayer(){
	IPaddress ip;
	int i;
//	TCPsocket tcpsock;
	bool error=false;
	char iptext[48];
	for(i=0;i<MAXNETPLAYERS;i++) mpstatus[i]=0;
	int valinta=joinMenu(iptext);
	entterpressed=false;
	set2=SDLNet_AllocSocketSet(1);
	if(valinta==1){	
		if(SDLNet_ResolveHost(&ip,iptext,9999)==-1) {
			dispErrorFullScreen();
			error=true;
		}
		if(!error){
			joinsoketti=SDLNet_TCP_Open(&ip);
			int numused=SDLNet_TCP_AddSocket(set2,joinsoketti);
			if(numused==-1) error=true;

		}

		if(!error){
			if(!joinsoketti) {
				dispErrorFullScreen();
				error=true;
			}
			else{
				int len,result;
				char *msg="Hello!;";
				len=strlen(msg)+1; // add one for the terminating NULL
				result=SDLNet_TCP_Send(joinsoketti,msg,len);
				if(result<len) {
					printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
					printf("yritettiin sanoa hello hostille\n");
				}
				char msg2[MAXLEN];
				int temp;
				for(temp=0;temp<MAXLEN;temp++){
					msg2[temp]='\0';
				}
				int started=1;
				bool saatu=false;
				while(started>0){
					started++;
					int numready;
					numready=SDLNet_CheckSockets(set2, 10);


					if(numready) {
						printf("There are %d joinsockets with activity!\n",numready);
						// check all sockets with SDLNet_SocketReady and handle the active ones.
						if(SDLNet_SocketReady(joinsoketti)) {
							result=SDLNet_TCP_Recv(joinsoketti,msg2,25);
							if(result<=0) {
								// An error may have occured, but sometimes you can just ignore it
								// It may be good to disconnect sock because it is likely invalid now.
								printf("hostilta saatu joinattaessa -1 byte‰\n");
							}
							//printf("Received: \"%s\"\n",msg2);
							// play with the client.
							if(compare(msg2,"you have joined the game",10)){
								started=-100;
								saatu=true;
								printf("game join signal received\n");
							}
						}
						if(started>1)started=-2;
					}
				}
				if(saatu){
					joinpeli=true;
					joinwaitforstart();
				}
				else{
					printf("game join singnal not received");
				}
			}
		}
	}

	if(joinsoketti!=NULL) SDLNet_TCP_Close(joinsoketti);
	SDLNet_FreeSocketSet(set2);
}

void drawJoinWait(int selection, int rivi){
	startMenu();
	glPushMatrix();
		glColor4f(1.0f,1.0f,1.0f,1.0f);		
		glBindTexture(GL_TEXTURE_2D, texture[29]);				
	float tempkoko=3.0f/4.0f;
//		float tempkoko=(float)screenheight/(float)screenwidth;
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f, -1.0f*tempkoko, 13);
			glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f, -1.0f*tempkoko, 13);
			glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f,  1.0f*tempkoko, 13);
			glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f,  1.0f*tempkoko, 13);
		glEnd();

		glColor4f(1,1,1,1);
/*		glBindTexture(GL_TEXTURE_2D, texture[18]);				
		glPushMatrix();
			if(rivi==0){
				glTranslatef(-8.1,5.65-selection*0.6,0);
			}else if(rivi==1){
				glTranslatef( 4.0,5.65-selection*0.6,0);
			}
			tempkoko=0.6;
			glBegin(GL_QUADS);
				glNormal3f(0,0,1.0f);
				glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*tempkoko, -1.0f*tempkoko, 0);
				glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*tempkoko, -1.0f*tempkoko, 0);
				glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*tempkoko,  1.0f*tempkoko, 0);
				glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*tempkoko,  1.0f*tempkoko, 0);
			glEnd();
		glPopMatrix();*/
		glColor4f(1,0,0,1);
		char *vekotin[]={
						"settings",
						"chat",
						"exit"};

		int i;
		for(i=0;i<3;i++){
			gl_println( -33,  28-i*3, -60,vekotin[i]);
		}
		char luku[15];
		for(i=0;i<playerslots;i++){
			//sprintf(luku,"player %i",i+1);
			if(mpstatus[i]==0){
				glColor4f(0.2,1,0.2,1);
				gl_println(  8,  28-i*3, -60, "open");
			}
			else if(mpstatus[i]==1){
				glColor4f(1,0.2,0.2,1);
				gl_println(  8,  28-i*3, -60, "closed");
			}
			else if(mpstatus[i]==2){
				glColor4f(1,0.2,1,1);
				gl_println(  8,  28-i*3, -60, "cpu");
			}
			else if(mpstatus[i]==3){
				glColor4f(0.2,0.2,1,1);
				gl_println(  8,  28-i*3, -60, "human");
			}

			sprintf(luku,netnick[i]);
			gl_println( -14,  28-i*3, -60, luku);

			if(mpstatus[i]==2 || mpstatus[i]==3)  sprintf(luku,"team %i", mptiimi[i]+1);
			else sprintf(luku,"team %i", 0);

			gl_println( 23,  28-i*3, -60, luku);

		}
		glColor4f(1,0,0,1);
		gl_println( -37,  -25, -60,"game mode:",1.2);
		glColor4f(0,0,1,1);
		if(gamemode==DEATHMATCHRANDOM){
			gl_println( -13,-25, -60,"death match with random ships",1);
		}else if(gamemode==DOMINATION){
			gl_println( -13,-25, -60,"flower domination",1);
		}else if(gamemode==CTF){
			gl_println( -13,-25, -60,"capture the flower",1);
		}else if(gamemode==MUSHROOM){
			gl_println( -13,-25, -60,"eat the mushroom",1);
		}else{
			gl_println( -13,-25, -60,"death match",1);
		}

GLfloat specular[4] = {2, 2, 2, 1};
glMaterialfv(GL_FRONT, GL_SPECULAR, specular);
glMaterialf(GL_FRONT, GL_SHININESS, 120);	
glLightfv(GL_LIGHT0,GL_SPECULAR,specular);
GLfloat lightdiffuse[4]= {0.5, 0.5, 0.5, 0.5};
glLightfv(GL_LIGHT0,GL_DIFFUSE, lightdiffuse);
GLfloat lightposition[4] = {0, 0, 2, 0};
glLightfv(GL_LIGHT0,GL_POSITION,lightposition);
//glGetLightfv(GL_LIGHT0,GL_POSITION,lightposition);
//printf("%f,%f,%f,%f\n",lightposition[0],lightposition[1],lightposition[2],lightposition[3]);

  glPushMatrix();
		glEnable(GL_LIGHTING);
		glDisable(GL_BLEND);
		glEnable(GL_DEPTH_TEST);
		glDisable(GL_TEXTURE_2D);

			if(rivi==0){
				glTranslatef(-7.7,5.62-selection*0.6,0);
			}else if(rivi==1){
				glTranslatef( 3.3,5.62-selection*0.6,0);
			}
		glRotatef(nuolirot,1,0,0);
		glScalef(0.22,0.3,0.3);
		glCallList(NUOLI);
		nuolirot+=2.5;
		if(nuolirot>180)nuolirot-=360;
	glPopMatrix();

	glPopMatrix();
	stopMenu();
}

void joinwaitforstart(){
	gamequited=false;
	playerslots=0;
	bool menuready=false , connect=false , exitmenu=false;
	entterpressed=false; spacepressed=false;
	player[0].pfire=false; player[0].palas=false; player[0].pright=false; player[0].pleft=false;
	int selectionmax=2;	int selection=0;
	int maxselectionrivi=1; int selectionrivi=0; 
	started2=0;

	while(!started2 && !exitmenu && !gamequited){
		while(!menuready){
			processEvents();
			if(started2 || gamequited) menuready=true;
			if(entterpressed || spacepressed){
				entterpressed=false;
				spacepressed=false;
				menuready=true;
			}else if(player[0].pleft){
				if(selectionrivi>0){
					selectionrivi--;
					if(selectionrivi==0){
						selectionmax=2;
						if(selection>selectionmax)selection=selectionmax;
					}

				}else{
					selectionmax=playerslots-1;
					selectionrivi=maxselectionrivi;
				}
				player[0].pleft=false;
			}else if(player[0].pright){
				if(selectionrivi<maxselectionrivi){
					selectionmax=playerslots-1;
					selectionrivi++;
				}else{
					selectionmax=0;
					if(selection>selectionmax)selection=selectionmax;
					selectionrivi=0;
				}
				player[0].pright=false;
			}else if(player[0].pylos){
				if(selection>0) selection--;
				else selection=selectionmax;
				player[0].pylos=false;
			}else if(player[0].palas){
				if(selection<selectionmax) selection++;
				else selection=0;
				player[0].palas=false;
			}else if(escpressed){
				selection=selectionmax;
				selectionrivi=0;
				menuready=true;
				escpressed=false;
			}
			do{
				currenttime=SDL_GetTicks()-starttime;
				framecounter=calculatefps*currenttime/1000.0;
				if(oldframecounter==framecounter) SDL_Delay(5);
			}	while (oldframecounter==framecounter);
			oldframecounter=framecounter;
			kuuntelejoin();
			drawJoinWait(selection, selectionrivi);
			updateAudio();
		}
		menuready=false;
		entterpressed=false;
		spacepressed=false;
		if(started2){
		}else if(selection==selectionmax && selectionrivi==0){
			exitmenu=true;
		}else if(selection==0 && selectionrivi==0){
			netGameSets(false);
		}else if(selection==1 && selectionrivi==0){
			chatScreen();
		}else if(selectionrivi==1){
			char msg[10];
			sprintf(msg,"team");
			msg[4]=(char)selection;
			msg[5]='\0';
			int len=6;
			int result=SDLNet_TCP_Send(joinsoketti,msg,len);
			if(result<len) {
				printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
			}else{
			}

		}
		

	}
	if(started2){
		int result;
		alkujuttusaatu=false;
		int error2=0;
		while(!alkujuttusaatu){
			kuuntelejoin();
			SDL_Delay(2);
			char msg4[20];
			sprintf(msg4,"alku huomattu");
			int len4=strlen(msg4)+1;
			result=SDLNet_TCP_Send(joinsoketti,msg4,len4);
			if(result<len4) {
				printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
				printf("yritettiin sanoa alku huomattu hostille\n");
				SDLNet_TCP_Close(joinsoketti);
				alkujuttusaatu=true;
				error2=1;
			}else{
				//printf("l‰hetettiin hostille %s\n",msg4);
			}
		}
		if(error2) printf("errori1\n %c", (char)error2);
		valmiinaalottamaan=false;
		if(!error2)while(!valmiinaalottamaan){
			kuuntelejoin();
			SDL_Delay(2);
			char msg4[20];
			sprintf(msg4,"tiimit saatu");
			int len4=strlen(msg4)+1;
			result=SDLNet_TCP_Send(joinsoketti,msg4,len4);
			if(result<len4) {
				printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
				printf("yritettiin sanoa tiimit saatu hostille\n");
			}else{
				printf("l‰hetettiin hostille %s\n",msg4);
			}
		}
		if(error2) printf("errori2:%i\n ",error2);
		if(!error2){
			int i;
			players=0;
			for(i=0;i<playerslots;i++){
				if(mpstatus[i]>1){
					if(players!=i) memcpy(&netnick[players],&netnick[i],11);
					players++;
				}
			}
			delete[] player;
			player=new Player[players];
			for(i=0;i<players;i++){
				double temp3=random(1000);
				player[i].asetaAlus(alus[1]);
				player[i].uudestaan(alus[(int)(temp3/1000.0f*(double)aluksia)]);
				player[i].kuolema=0;
				player[i].numero=i;
				player[i].pylos=false;
				player[i].pfire=false;
				player[i].pease=false;
				player[i].palas=false;
				player[i].pleft=false;
				player[i].pright=false;
				player[i].pfire=false;
				player[i].pisteet=fullcredits;
				player[i].tiimi=mptiimi[i];
				if(mpstatus[i]==2)player[i].ihmispelaaja=false;
				else player[i].ihmispelaaja=true;
			}
			netJoinPeli();
		}
	}
}


void kuuntelejoin(){

	int i;
	int result;

	if(SDL_GetTicks() >= (unsigned int)(timeouttarkistettu+timeoutaika)){
		timeouttarkistettu=SDL_GetTicks();
		char msg4[10];
		sprintf(msg4,"Moi!");
		int len4=strlen(msg4)+1;
		result=SDLNet_TCP_Send(joinsoketti,msg4,len4);
		if(result<len4) {
			printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
			printf("yritettiin sanoa moi hostille\n");
		}else{
			//printf("l‰hetettiin hostille %s\n",msg4);
		}
	}


	char msg[MAXLEN];
	int numready=SDLNet_CheckSockets(set2, 10);
	if(numready) {
		//printf("There are %d joinsockets with activity!\n",numready);
		if(SDLNet_SocketReady(joinsoketti)) {
			for(i=0;i<MAXLEN;i++){
				msg[i]=0;
			}
			result=SDLNet_TCP_Recv(joinsoketti,msg,MAXLEN);
			if(result<=0) {
				gamequited=true;
			}
			//printf("Received: \"%s\"\n",msg);
			// play with the client.
			joinwaitKasittele(msg, result);
		}
	}
}


void joinwaitKasittele(char *msg, int result2){
	int len,result,i,n=0;

	if(result2>7 && compare(msg,"tiimit",6)){
		char temp=msg[6];
		playerslots=(int)temp;
		//printf("tiimej‰ yritet‰‰n s‰‰t‰, slotteja on %i\n",playerslots);
		for(i=0;i<playerslots;i++){
			mptiimi[i]=0;
			mpstatus[i]=0;
		}
		n=7;
		for(i=0;i<playerslots;i++){
			if(result2>n+12){
				mptiimi[i]=msg[n];
				n++;
				mpstatus[i]=msg[n];
				n++;
				memcpy(&netnick[i],&msg[n],11);
				n+=11;
			}else return;
		}

		//AlkuTiedot tiedot;
		//memcpy(&tiedot,&msg[n],sizeof(AlkuTiedot));
		//n+=sizeof(AlkuTiedot);
		char tgamemode;
		short tmpcredits;
		float tfps;
		if(result2>n+6){
			memcpy(&tgamemode,&msg[n],sizeof(char));
			n+=sizeof(char);
			memcpy(&tmpcredits,&msg[n],sizeof(short));
			n+=sizeof(short);
			memcpy(&tfps,&msg[n],sizeof(float));
			n+=sizeof(float);
		}else return;

		gamemode=tgamemode;
		mpcredits=tmpcredits;
		calculatefps=tfps;

//		printf("g=%i,c=%i,f=%f\n",tgamemode,tmpcredits,tfps);
		//gamemode=tiedot.gamemode;
		//mpcredits=tiedot.mpcredits;

		if(started2){
			alkujuttusaatu=true;
			printf("tiimit saatu lopullisesti\n");
		}
		//n++;

	}
	else if(result2>16 && compare(msg,"pelaajia montako",10)){
		char msg3[60];
		sprintf(msg3,"pelaajia ");
		char luku5=joinaavia;
		if(luku5>4) luku5=4;
		msg3[9]=luku5;
		n=10;
		int index;
		for(index=0;index<joinaavia;index++){
			msg3[n]=11;
			n++;
			memcpy(&msg3[n],sets->nick[index],11);
			//printf("sets->nick %i=%s\n",index,sets->nick[index]);
			n+=11;
		}
		msg3[n]='\0';
		len=12*joinaavia+11;
		result=SDLNet_TCP_Send(joinsoketti,msg3,len);
		if(result<len) {
			printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
			printf("yritettiin kertoa pelaajam‰‰r‰‰ hostille\n");
		}else{
			printf("l‰hetettiin hostille %i pelaajaa\n",luku5);
		}
	}
	else if(result2>15 && compare(msg,"pelaajia liikaa!",15)){
		gamequited=true;
		dispCustomError("ei mahtunut mukaan");
		escpressed=true;
		//pelaajialiikaaerror=true;
		n=16;
	}

	else if(result2>12 && compare(msg,"game started",7)){
		started2=1;
		n=12;
		printf("game start signal received\n");
	}else if(result2>7 && compare(msg,"quiting",5)){
		gamequited=true;
		n=7;
	}else if(result2>15 && compare(msg,"kaikki kunnossa",6)){
		valmiinaalottamaan=true;
		n=15;
	}else if(result2>3 && (compare(msg,"p2a",3) || compare(msg, "pal",3))){
		kasittelePeliMsg(msg, result2);
		return;
	}
	else if(result2>10 && compare(msg,"x‰t",3)){
		lisaaRivi(&msg[3],strlen((const char*)&msg[3]-1));
		n+=strlen((const char*)&msg[3]-1)+3;
	}

	if((result2-n)>6){
		for(i=0;i<6;i++){
			if(msg[n]==';'){
				n++;
				if(!(msg[n]<='z' && msg[n]>='a'))n++;
				if(!(msg[n]<='z' && msg[n]>='a'))n++;
				if(!(msg[n]<='z' && msg[n]>='a'))n++;
				if(result2-n>5){
					joinwaitKasittele(&msg[n], result2-n);
				}
			}
			n++;
		}
	}
}
