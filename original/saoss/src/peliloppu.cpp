#include "main.h"
#include "peliloppu.h"
#include "net.h"

int peliloppu(){
	int i;
	if(gamemode==DEATHMATCH || gamemode==DEATHMATCHRANDOM){
		int ekatiimi=-1;
		for(i=0;i<players;i++){
			if(player[i].pisteet>=0){
				ekatiimi=player[i].tiimi;
				break;
			}
		}
		if(ekatiimi<0) return 1;
		for(i=0;i<players;i++){
			if(player[i].pisteet>=0 && player[i].tiimi!=ekatiimi) return 0;
		}
		return 1;
	}else if(gamemode==DOMINATION || gamemode==CTF || gamemode==MUSHROOM){
		for(i=0;i<players;i++){
			if(player[i].pisteet>=mpcredits) return 1;
		}
	}
	return 0;
}

void netVictory(){
	int i;
	int len,result,team=-1;
	int n=0;
	char pcount2=0;
	if(gamemode==DOMINATION || gamemode==CTF || gamemode==MUSHROOM){
		char msg[100];
		sprintf(msg,"victoryo");
		n+=8;
		for(i=0;i<players;i++){
			if(player[i].pisteet>=mpcredits){
				team=player[i].tiimi;
				i=players;
			}
		}
		msg[n]=team;
		n++;
		msg[n]=';';
		msg[n+1]='\0';
		n+=2;
		len=n;
		for(i=1;i<playerslots;i++){
			if(tcpsoketit[i]!=NULL){
				result=SDLNet_TCP_Send(tcpsoketit[i],msg,len);
				if(result<len) {
					printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
					// It may be good to disconnect sock because it is likely invalid now.
				}
			}
		}
		netVictoryLoop();
	}else if(gamemode==DEATHMATCH || gamemode==DEATHMATCHRANDOM){
		char msg[100];
		sprintf(msg,"victoryd");
		n+=8;

		for(i=0;i<players;i++){
			if(player[i].pisteet>=0){
				pcount2++;
				team=player[i].tiimi;
			}
		}
		msg[n]=team;
		n++;
		msg[n]=pcount2;
		n++;
		for(i=0;i<players;i++){
			if(player[i].pisteet>=0){
				VictoryPlayer vp;
				vp.numero=i;
				vp.pisteet=player[i].pisteet;
				memcpy(&msg[n],&vp,sizeof(VictoryPlayer));
				n+=sizeof(VictoryPlayer);
			}
		}
		msg[n]=';';
		msg[n+1]='\0';
		n+=2;
		len=n;
		for(i=1;i<playerslots;i++){
			if(tcpsoketit[i]!=NULL){
				result=SDLNet_TCP_Send(tcpsoketit[i],msg,len);
				if(result<len) {
					printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
					// It may be good to disconnect sock because it is likely invalid now.
				}
			}
		}
		netVictoryLoop();
	}
}

void netVictoryLoop(){
	entterpressed=false;
	escpressed=false;
	while (!entterpressed && !escpressed){
		if(hostpeli) drawNetVictoryLoop();
		else drawJoinNetVictoryLoop();
		processEvents();
		do{
			currenttime=SDL_GetTicks()-starttime;
			framecounter=calculatefps*currenttime/1000.0;
		}	while (oldframecounter==framecounter);
		updateAudio();
	}
	entterpressed=false;
	escpressed=false;
}
void drawJoinNetVictoryLoop(){
	glDisable(GL_LIGHTING);
	glDisable(GL_BLEND);
	glEnable(GL_TEXTURE_2D);
	gluPerspective(45.0f,(GLfloat)screenwidth/(GLfloat)screenheight,1.0f,100.0f);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);			
	glLoadIdentity();							
	glMatrixMode(GL_MODELVIEW);

	gluLookAt(0,0,15,
	0,0,0,
	0,1,0);


	char numero4[3];
	int numerotemp=0;
	int i;
	int pcount=0,pcount2=0;


	if(gamemode==DEATHMATCH || gamemode==DEATHMATCHRANDOM){
		char numero2[3];
		itoa( voittotiimi+1, numero2, 10);
		for(i=0;i<players;i++){
			if(player[i].pisteet>=0){
				pcount2++;
			}
		}
		if(pcount2<5){
			glScalef(0.5,0.5,0.5);
		}else glScalef(0.32,0.32,0.32);
		for(i=0;i<players;i++){
			if(player[i].pisteet>=0 && player[i].tiimi==voittotiimi){
				numerotemp+=player[i].pisteet+player[i].alus.pistemaara;
				player[i].piirra((pcount2-1)*(-4)+8*pcount,-6, -3, 90, 0);
				pcount++;
			}
		}
		if(pcount2<5){
			glScalef(2,2,2);
		}else glScalef(3.125,3.125,3.125);
		itoa(numerotemp, numero4, 10);
		char *vekotin[]={"team",
						numero2,
						"won",
						"with",
						numero4,
						"credits"};
		glColor4f(1,0,0,1);
		if(pcount2>0){
			gl_println(-17, 12,-25,vekotin[0]);
			gl_println( -4, 12,-25,vekotin[1]);
			gl_println(  1, 12,-25,vekotin[2]);	

			gl_println(-16, 6,-25,vekotin[3]);
			gl_println( -2, 6,-25,vekotin[4]);
			gl_println(  6, 6,-25,vekotin[5]);
		}else{
			gl_println(-4, 0,-25,"nobody won");
		}
	}else if(gamemode==DOMINATION || gamemode==CTF || gamemode==MUSHROOM){
		char numero2[3];
		char team=voittotiimi;
		itoa( voittotiimi+1, numero2, 10);

		for(i=0;i<players;i++){
			if(player[i].tiimi==team){
				pcount2++;
			}
		}

		if(pcount2<5){
			glScalef(0.5,0.5,0.5);
		}else glScalef(0.32,0.32,0.32);

		for(i=0;i<players;i++){
			if(player[i].tiimi==team){
				player[i].piirra((pcount2-1)*(-4)+8*pcount,-6, -3, 90, 0);
				pcount++;
			}
		}
		if(pcount2<5){
			glScalef(2,2,2);
		}else glScalef(3.125,3.125,3.125);
		itoa(numerotemp, numero4, 10);

		char *vekotin[]={"team",
						numero2,
						"won",
						"with",
						numero4,
						"credits"};
		if(pcount2>0){
			glColor4f(1,0,0,1);
			gl_println(-17, 12,-25,vekotin[0]);
			gl_println( -4, 12,-25,vekotin[1]);
			gl_println(  1, 12,-25,vekotin[2]);
		}else{
			gl_println(-4, 0,-25,"nobody won");
		}

//		gl_println(-16, 6,-25,vekotin[3]);
//		gl_println( -2, 6,-25,vekotin[4]);
//		gl_println(  6, 6,-25,vekotin[5]);
	}




	glEnable(GL_BLEND);
//	glEnable(GL_LIGHTING);
//	glDisable(GL_TEXTURE_2D);
	SDL_GL_SwapBuffers();
}
void drawNetVictoryLoop(){
	glDisable(GL_LIGHTING);
	glDisable(GL_BLEND);
	glEnable(GL_TEXTURE_2D);
	gluPerspective(45.0f,(GLfloat)screenwidth/(GLfloat)screenheight,1.0f,100.0f);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);			
	glLoadIdentity();							
	glMatrixMode(GL_MODELVIEW);

	gluLookAt(0,0,15,
	0,0,0,
	0,1,0);


	char numero4[3];
	int numerotemp=0;
	int i;
	int pcount=0,pcount2=0;


	if(gamemode==DEATHMATCH || gamemode==DEATHMATCHRANDOM){
		char numero2[3];
		for(i=0;i<players;i++){
			if(player[i].pisteet>=0){
				itoa( player[i].tiimi+1, numero2, 10);
				break;
			}
		}
		for(i=0;i<players;i++){
			if(player[i].pisteet>=0){
				pcount2++;
			}
		}
		if(pcount2<5){
			glScalef(0.5,0.5,0.5);
		}else glScalef(0.32,0.32,0.32);
		for(i=0;i<players;i++){
			if(player[i].pisteet>=0){
				numerotemp+=player[i].pisteet+player[i].alus.pistemaara;
				player[i].piirra((pcount2-1)*(-4)+8*pcount,-6, -3, 90, 0);
				pcount++;
			}
		}
		if(pcount2<5){
			glScalef(2,2,2);
		}else glScalef(3.125,3.125,3.125);
		itoa(numerotemp, numero4, 10);
		char *vekotin[]={"team",
						numero2,
						"won",
						"with",
						numero4,
						"credits"};
		glColor4f(1,0,0,1);
		if(pcount2>0){
			gl_println(-17, 12,-25,vekotin[0]);
			gl_println( -4, 12,-25,vekotin[1]);
			gl_println(  1, 12,-25,vekotin[2]);

			gl_println(-16, 6,-25,vekotin[3]);
			gl_println( -2, 6,-25,vekotin[4]);
			gl_println(  6, 6,-25,vekotin[5]);
		
		}else{
			gl_println(-4, 0,-25,"draw");
		}

	}else if(gamemode==DOMINATION || gamemode==CTF || gamemode==MUSHROOM){
		char numero2[3];
		char team=-1;
		for(i=0;i<players;i++){
			if(player[i].pisteet>=mpcredits){
				itoa( player[i].tiimi+1, numero2, 10);
				team=player[i].tiimi;
				break;
			}
		}
		for(i=0;i<players;i++){
			if(player[i].tiimi==team){
				pcount2++;
			}
		}

		if(pcount2<5){
			glScalef(0.5,0.5,0.5);
		}else glScalef(0.32,0.32,0.32);

		for(i=0;i<players;i++){
			if(player[i].tiimi==team){
				player[i].piirra((pcount2-1)*(-4)+8*pcount,-6, -3, 90, 0);
				pcount++;
			}
		}
		if(pcount2<5){
			glScalef(2,2,2);
		}else glScalef(3.125,3.125,3.125);
		itoa(numerotemp, numero4, 10);

		char *vekotin[]={"team",
						numero2,
						"won",
						"with",
						numero4,
						"credits"};
		glColor4f(1,0,0,1);
		if(pcount2>0){
			gl_println(-17, 12,-25,vekotin[0]);
			gl_println( -4, 12,-25,vekotin[1]);
			gl_println(  1, 12,-25,vekotin[2]);
		
		}else{
			gl_println(-4, 0,-25,"draw");
		}


//		gl_println(-16, 6,-25,vekotin[3]);
//		gl_println( -2, 6,-25,vekotin[4]);
//		gl_println(  6, 6,-25,vekotin[5]);
	}




	glEnable(GL_BLEND);
//	glEnable(GL_LIGHTING);
//	glDisable(GL_TEXTURE_2D);
	SDL_GL_SwapBuffers();
}

void drawVictory(int numero){
	glDisable(GL_LIGHTING);
	glDisable(GL_BLEND);
	glEnable(GL_TEXTURE_2D);
	gluPerspective(45.0f,(GLfloat)screenwidth/(GLfloat)screenheight,1.0f,100.0f);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);			
	glLoadIdentity();							
	glMatrixMode(GL_MODELVIEW);

	gluLookAt(0,0,15,
	0,0,0,
	0,1,0);

	int numerotemp=0;
	int i;
	if(gamemode!=CTF && gamemode!=DOMINATION && gamemode!=MUSHROOM){
		char numero2[2];
		itoa( numero+1, numero2, 10);

		char numero4[3];

		int pcount=0,pcount2=0;
		for(i=0;i<players;i++){
			if(player[i].pisteet>=0){
				pcount2++;
			}
		}
		if(pcount2<5){
			glScalef(0.5,0.5,0.5);
		}else glScalef(0.32,0.32,0.32);
		for(i=0;i<players;i++){
			if(player[i].pisteet>=0){
				numerotemp+=player[i].pisteet+player[i].alus.pistemaara;
				player[i].piirra((pcount2-1)*(-4)+8*pcount,-6, -3, 90, 0);
				pcount++;
			}
		}
		if(pcount2<5){
			glScalef(2,2,2);
		}else glScalef(3.125,3.125,3.125);
		itoa(numerotemp, numero4, 10);		

		char *vekotin[]={"team",
						numero2,
						"won",
						"with",
						numero4,
						"credits"};
		glColor4f(1,0,0,1);
		if(pcount2>0){
			gl_println(-17, 12,-25,vekotin[0]);
			gl_println( -4, 12,-25,vekotin[1]);
			gl_println(  1, 12,-25,vekotin[2]);	

			gl_println(-16, 6,-25,vekotin[3]);
			gl_println( -2, 6,-25,vekotin[4]);
			gl_println(  6, 6,-25,vekotin[5]);
		}else{
			gl_println(-4, 0,-25,"draw");
		}
	}else{
		int pcount=0,ptiimi=-1,pcount2=0;
		for(i=0;i<players;i++){
			if(player[i].pisteet>=fullcredits)ptiimi=player[i].tiimi;
		}
		for(i=0;i<players;i++){
			if(player[i].tiimi==ptiimi){
				pcount2++;
			}
		}

		if(pcount2<5){
			glScalef(0.5,0.5,0.5);
		}else glScalef(0.32,0.32,0.32);

		for(i=0;i<players;i++){
			if(player[i].tiimi==ptiimi){
				player[i].piirra((pcount2-1)*(-4)+8*pcount,-6, -3, 90, 0);
				pcount++;
			}
		}
		if(pcount2<5){
			glScalef(2,2,2);
		}else glScalef(3.125,3.125,3.125);

		char numero2[3];
		sprintf(numero2,"%i",ptiimi);

		char *vekotin[]={"team",
						numero2,
						"won"};
		glColor4f(1,0,0,1);
		if(pcount2>0){
			gl_println(-17, 12,-25,vekotin[0]);
			gl_println( -4, 12,-25,vekotin[1]);
			gl_println(  1, 12,-25,vekotin[2]);	
		}else{
			gl_println(-4, 0,-25,"draw");
		}

	}


	glEnable(GL_BLEND);
//	glEnable(GL_LIGHTING);
//	glDisable(GL_TEXTURE_2D);
	SDL_GL_SwapBuffers();

}
