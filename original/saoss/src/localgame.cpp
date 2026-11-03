#include "main.h"
#include "net.h"
#include "scene.h"
#include "domination.h"
#include "ctf.h"
#include "sieni.h"

void onePlayerGameMenu(){
	bool menuready=false;
	bool menuquit=false;
	int selection=0;
	bool pistevoitto=false;
	int selectionmax=8;
	saavalita=true;
	//printf("efects=%i\n",sets->efects);
	gamemode=DEATHMATCHRANDOM;

	while(!menuquit){
		//itse menu
		while(!menuready){
			processEvents();
			joinpeli=false; hostpeli=false;
			if(entterpressed){
				entterpressed=false;
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
			drawOnePlayerGameMenu(selection);
			updateAudio();
		}
		menuready=false;

		//game random-aluksilla
		if(selection==0){

			gamemode=DEATHMATCHRANDOM;
			int i;
			pistepeli=false;
			saavalita=false;
			for(i=0;i<players;i++){
				player[i].kuolema=0;
				player[i].pisteet=0;
			}
			ekapisteframe=true;
			for(i=0;i<players;i++){
				double temp3=random(10000);
				int temp3int=(int)(temp3/10001.0f*(double)aluksia);
				if(temp3int>=aluksia)temp3int-=aluksia;
				player[i].uudestaan(alus[temp3int]);
			}
//			player[1].uudestaan(alus[22]);
//			player[0].uudestaan(alus[15]);

			while(!escpressed){
				do{
					currenttime=SDL_GetTicks()-starttime;
					framecounter=calculatefps*currenttime/1000.0;
					if(oldframecounter==framecounter) SDL_Delay(2);
				}	while (oldframecounter==framecounter);
				skipframes=framecounter-oldframecounter;
				for (;skipframes>0;skipframes--){
					calculatedframes++;
					calculateFrame(++oldframecounter);
				}
				processEvents();
				drawedframes++;
				drawFrame(framecounter);
				framesdrawn++;
				updateAudio();
			}
		}

		//game ilman alusten uudelleen laittoa (resume)
		else if(selection==1) {
			int i;
			//resumetetaan pistepeli
			if(pistepeli){

				pistepeli=true;
				ekapisteframe=false;
				joinpeli=false;
				hostpeli=false;
				ekapisteframe=true;

				gamemode=DEATHMATCH;
				while(!escpressed){
					do{
						currenttime=SDL_GetTicks()-starttime;
						framecounter=calculatefps*currenttime/1000.0;
						if(oldframecounter==framecounter) SDL_Delay(2);
					}	while (oldframecounter==framecounter);
					if(oldframecounter<framecounter-5)oldframecounter=framecounter;
					skipframes=framecounter-oldframecounter;
					for (;skipframes>0;skipframes--){
						calculateFrame(++oldframecounter);
						calculatedframes++;
					}
					int lastteam=-1,lapi=0;
					for(i=0;i<players;i++){
						if(player[i].pisteet>=0 && player[i].tiimi!=lastteam){
							lapi++;
							lastteam=player[i].tiimi;
						}else if(player[i].pisteet>=0){
//							lapi++;
						}
					}
					if(lapi<2){
							escpressed=true;
							pistevoitto=true;
					}
					processEvents();
					drawedframes++;
					drawFrame(framecounter);
					framesdrawn++;
					updateAudio();
				}
				if(pistevoitto){
					pistevoitto=false;
					int j=-2,k=0;
					for(i=0;i<players;i++){
						if(player[i].pisteet>j){
							j=player[i].pisteet;
							k=player[i].tiimi;
						}
					}
					while (!entterpressed){
						drawVictory(k);
						processEvents();
						do{
							currenttime=SDL_GetTicks()-starttime;
							framecounter=calculatefps*currenttime/1000.0;
							if(oldframecounter==framecounter) SDL_Delay(2);
						}	while (oldframecounter==framecounter);
					updateAudio();
					}
					entterpressed=false;
				}

			}

			//resumetetaan randomgame
			else while(!escpressed){

				pistepeli=false;
				ekapisteframe=false;
				joinpeli=false;
				hostpeli=false;
				ekapisteframe=true;

//				gamemode=DEATHMATCH;
				
				
/*				joinpeli=false;
				hostpeli=false;
				saavalita=false;
				for(i=0;i<players;i++){
					player[i].kuolema=0;
					player[i].pisteet=1000;
				}*/
				ekapisteframe=true;
				gamemode=DEATHMATCHRANDOM;
				do{
					currenttime=SDL_GetTicks()-starttime;
					framecounter=calculatefps*currenttime/1000.0;
					if(oldframecounter==framecounter) SDL_Delay(2);
				}	while (oldframecounter==framecounter);
				skipframes=framecounter-oldframecounter;
				for (;skipframes>0;skipframes--){
					calculateFrame(++oldframecounter);
					calculatedframes++;
				}
				processEvents();
				drawFrame(framecounter);
				drawedframes++;
				framesdrawn++;	
				updateAudio();
			}
		}

		//pistepeli, saa valita alukset..
		else if(selection==2){
			gamemode=DEATHMATCH;
			int i;
			pistepeli=true;
			saavalita=true;
			for(i=0;i<players;i++){
				player[i].kuolema=0;
				player[i].pisteet=fullcredits;
			}
			ekapisteframe=true;
			while(!escpressed){
				do{
					currenttime=SDL_GetTicks()-starttime;
					framecounter=calculatefps*currenttime/1000.0;
					if(oldframecounter==framecounter) SDL_Delay(2);
				}	while (oldframecounter==framecounter);
				if(oldframecounter<framecounter-5)oldframecounter=framecounter;
				skipframes=framecounter-oldframecounter;
				for (;skipframes>0;skipframes--){
					calculateFrame(++oldframecounter);
					calculatedframes++;
				}
				int lastteam=-1,lapi=0;
				for(i=0;i<players;i++){
					if(player[i].pisteet>=0 && player[i].tiimi!=lastteam){
						lapi++;
						lastteam=player[i].tiimi;
					}else if(player[i].pisteet>=0){
//						lapi++;
					}
				}
				if(lapi<2){
						escpressed=true;
						pistevoitto=true;
				}
				processEvents();
				drawedframes++;
				drawFrame(framecounter);
				framesdrawn++;
				updateAudio();
			}
			if(pistevoitto){
				pistevoitto=false;
				int j=-2,k=0;
				for(i=0;i<players;i++){
					if(player[i].pisteet>j){
						j=player[i].pisteet;
						k=player[i].tiimi;;
					}
				}
				while (!entterpressed){
					drawVictory(k);
					processEvents();
					do{
						currenttime=SDL_GetTicks()-starttime;
						framecounter=calculatefps*currenttime/1000.0;
						if(oldframecounter==framecounter) SDL_Delay(2);
					}	while (oldframecounter==framecounter);
				updateAudio();
				}
				entterpressed=false;
			}
		}

		//pistepeli, ei saa valita aluksia..
		else if(selection==3){
			gamemode=DEATHMATCHRANDOM;
			int i;
			pistepeli=true;
			saavalita=false;
			for(i=0;i<players;i++){
				player[i].kuolema=0;
				player[i].pisteet=fullcredits;
			}
			ekapisteframe=true;
			for(i=0;i<players;i++){
				double temp3=random(10000);
				int temp3int=(int)(temp3/10001.0f*(double)aluksia);
				if(temp3int>=aluksia)temp3int-=aluksia;
				player[i].uudestaan(alus[temp3int]);
			}
			while(!escpressed){
				do{
					currenttime=SDL_GetTicks()-starttime;
					framecounter=calculatefps*currenttime/1000.0;
					if(oldframecounter==framecounter) SDL_Delay(2);
				}	while (oldframecounter==framecounter);
				if(oldframecounter<framecounter-5)oldframecounter=framecounter;
				skipframes=framecounter-oldframecounter;
				for (;skipframes>0;skipframes--){
					calculatedframes++;
					calculateFrame(++oldframecounter);
				}
				int lastteam=-1,lapi=0;
				for(i=0;i<players;i++){
					if(player[i].pisteet>=0 && player[i].tiimi!=lastteam){
						lapi++;
						lastteam=player[i].tiimi;
					}else if(player[i].pisteet>=0){
//						lapi++;
					}
				}
				if(lapi<2){
						escpressed=true;
						pistevoitto=true;
				}
				processEvents();
				drawedframes++;
				drawFrame(framecounter);
				framesdrawn++;
				updateAudio();
			}
			if(pistevoitto){
				pistevoitto=false;
				int j=-2,k=0;
				for(i=0;i<players;i++){
					if(player[i].pisteet>j){
						j=player[i].pisteet;
						k=player[i].tiimi;;
					}
				}
				while (!entterpressed){
					drawVictory(k);
					processEvents();
					do{
						currenttime=SDL_GetTicks()-starttime;
						framecounter=calculatefps*currenttime/1000.0;
						if(oldframecounter==framecounter) SDL_Delay(2);
					}	while (oldframecounter==framecounter);
				updateAudio();
				}
				entterpressed=false;
			}
		}

		//domination
		else if(selection==4){
			gamemode=DOMINATION;
			int i;
			pistepeli=false;
			saavalita=false;
			for(i=0;i<players;i++){
				player[i].kuolema=0;
				player[i].pisteet=0;
			}
			for(i=0;i<dpisteita;i++){
				dpiste[i].tiimi=-1;
			}
			ekapisteframe=true;
			for(i=0;i<players;i++){
				double temp3=random(10000);
				int temp3int=(int)(temp3/10001.0f*(double)aluksia);
				if(temp3int>=aluksia)temp3int-=aluksia;
				player[i].uudestaan(alus[temp3int]);
			}
			while(!escpressed){
				do{
					currenttime=SDL_GetTicks()-starttime;
					framecounter=calculatefps*currenttime/1000.0;
					if(oldframecounter==framecounter) SDL_Delay(2);
				}	while (oldframecounter==framecounter);
				if(oldframecounter<framecounter-10)oldframecounter=framecounter-1;
				skipframes=framecounter-oldframecounter;
				for (;skipframes>0;skipframes--){
					calculatedframes++;
					calculateFrame(++oldframecounter);
				}
				int lapi=0;
				for(i=0;i<players;i++){
					if(player[i].pisteet>=fullcredits){
						lapi=2;
					}
				}
				if(lapi>1){
						escpressed=true;
						pistevoitto=true;
				}
				processEvents();
				drawedframes++;
				drawFrame(framecounter);
				framesdrawn++;
				updateAudio();
			}
			if(pistevoitto){
				pistevoitto=false;
				int j=-2,k=0;
				for(i=0;i<players;i++){
					if(player[i].pisteet>j){
						j=player[i].pisteet;
						k=player[i].tiimi;;
					}
				}
				while (!entterpressed){
					drawVictory(k);
					processEvents();
					do{
						currenttime=SDL_GetTicks()-starttime;
						framecounter=calculatefps*currenttime/1000.0;
						if(oldframecounter==framecounter) SDL_Delay(2);
					}	while (oldframecounter==framecounter);
				updateAudio();
				}
				entterpressed=false;
			}
		}

		//ctf
		else if(selection==5){
			gamemode=CTF;
			int i;
			pistepeli=false;
			saavalita=false;
			for(i=0;i<players;i++){
				player[i].kuolema=0;
				player[i].pisteet=0;
			}
			for(i=0;i<players;i++){
				cpiste[i].enabled=false;
				cpiste[i].x=alkupiste[i].x;
				cpiste[i].y=alkupiste[i].y;
				cpiste[player[i].tiimi].player=100;
			}
			for(i=0;i<players;i++){
				cpiste[player[i].tiimi].enabled=true;
			}
			for(i=0;i<players;i++){
				double temp3=random(10000);
				int temp3int=(int)(temp3/10001.0f*(double)aluksia);
				if(temp3int>=aluksia)temp3int-=aluksia;
				player[i].uudestaan(alus[temp3int]);
			}
			ekapisteframe=true;
			while(!escpressed){
				do{
					currenttime=SDL_GetTicks()-starttime;
					framecounter=calculatefps*currenttime/1000.0;
					if(oldframecounter==framecounter) SDL_Delay(2);
				}	while (oldframecounter==framecounter);
				if(oldframecounter<framecounter-5)oldframecounter=framecounter;
				skipframes=framecounter-oldframecounter;
				for (;skipframes>0;skipframes--){
					calculatedframes++;
					calculateFrame(++oldframecounter);
				}
				int lapi=0;
				for(i=0;i<players;i++){
					if(player[i].pisteet>=fullcredits){
						lapi=2;
					}
				}
				if(lapi>1){
					escpressed=true;
					pistevoitto=true;
				}
				processEvents();
				drawedframes++;
				drawFrame(framecounter);
				framesdrawn++;
				updateAudio();
			}
			if(pistevoitto){
				pistevoitto=false;
				int j=-2,k=0;
				for(i=0;i<players;i++){
					if(player[i].pisteet>j){
						j=player[i].pisteet;
						k=player[i].tiimi;;
					}
				}
				while (!entterpressed){
					drawVictory(k);
					processEvents();
					do{
						currenttime=SDL_GetTicks()-starttime;
						framecounter=calculatefps*currenttime/1000.0;
						if(oldframecounter==framecounter) SDL_Delay(2);
					}	while (oldframecounter==framecounter);
				updateAudio();
				}
				entterpressed=false;
			}
		}

		//mushroom
		else if(selection==6){
			gamemode=MUSHROOM;
			int i;
			pistepeli=false;
			saavalita=false;
			for(i=0;i<players;i++){
				player[i].kuolema=0;
				player[i].pisteet=0;
			}
			for(i=0;i<players;i++){
				spiste[i].x=0;
				spiste[i].y=50;
			}
			for(i=0;i<players;i++){
				double temp3=random(10000);
				int temp3int=(int)(temp3/10001.0f*(double)aluksia);
				if(temp3int>=aluksia)temp3int-=aluksia;
				player[i].uudestaan(alus[temp3int]);
			}
			ekapisteframe=true;
			while(!escpressed){
				do{
					currenttime=SDL_GetTicks()-starttime;
					framecounter=calculatefps*currenttime/1000.0;
					if(oldframecounter==framecounter) SDL_Delay(2);
				}	while (oldframecounter==framecounter);
				if(oldframecounter<framecounter-5)oldframecounter=framecounter;
				skipframes=framecounter-oldframecounter;
				for (;skipframes>0;skipframes--){
					calculatedframes++;
					calculateFrame(++oldframecounter);
				}
				int lapi=0;
				for(i=0;i<players;i++){
					if(player[i].pisteet>=fullcredits){
						lapi=2;
					}
				}
				if(lapi>1){
					escpressed=true;
					pistevoitto=true;
				}
				processEvents();
				drawedframes++;
				drawFrame(framecounter);
				framesdrawn++;
				updateAudio();
			}
			if(pistevoitto){
				pistevoitto=false;
				int j=-2,k=0;
				for(i=0;i<players;i++){
					if(player[i].pisteet>j){
						j=player[i].pisteet;
						k=player[i].tiimi;;
					}
				}
				while (!entterpressed){
					drawVictory(k);
					processEvents();
					do{
						currenttime=SDL_GetTicks()-starttime;
						framecounter=calculatefps*currenttime/1000.0;
						if(oldframecounter==framecounter) SDL_Delay(2);
					}	while (oldframecounter==framecounter);
				updateAudio();
				}
				entterpressed=false;
			}
		}
		else if(selection==7){
			asetusmenu();
		}

		//quit
		else if(selection==selectionmax){
			menuquit=true;
		}

		player[0].palas=false;
		player[0].pylos=false;
		player[0].pright=false;
		player[0].pleft=false;
		player[0].pfire=false;
		entterpressed=false;

		escpressed=false;
		//exitProgram(0);
	}
}

void drawOnePlayerGameMenu(int selection){
	glDisable(GL_LIGHTING);
	glEnable(GL_BLEND);
	glBlendFunc(GL_ONE,GL_ONE_MINUS_SRC_COLOR);	
	glDisable(GL_DEPTH_TEST);
	glEnable(GL_TEXTURE_2D);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);			
	gluPerspective(45.0f,(GLfloat)screenwidth/(GLfloat)screenheight,0.5f,80.0f);
	glLoadIdentity();							
	glMatrixMode(GL_MODELVIEW);


	glColor4f(1.0f,1.0f,1.0f,1.0f);		
	glBindTexture(GL_TEXTURE_2D, texture[31]);	//27			
	float tempkoko=3.0f/4.0f;
//	float tempkoko=(float)screenheight/(float)screenwidth;
	//float tempkoko=0.6f;
	glBegin(GL_QUADS);
		glNormal3f(0,0,1.0f);
		glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f, -1.0f*tempkoko, -1.8);
		glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f, -1.0f*tempkoko, -1.8);
		glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f,  1.0f*tempkoko, -1.8);
		glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f,  1.0f*tempkoko, -1.8);
	glEnd();

/*	glBegin(GL_QUADS);
		glNormal3f(0,0,1.0f);
		glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f, -1.0f*tempkoko, 13.2);
		glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f, -1.0f*tempkoko, 13.2);
		glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f,  1.0f*tempkoko, 13.2);
		glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f,  1.0f*tempkoko, 13.2);
	glEnd();
*/
	gluLookAt(0,0,15,
	0,0,0,
	0,1,0);
	char numero3[10];
	itoa( fullcredits, numero3, 10);

	char *vekotin[]={	"local game",
						"Start With Random Ships",
						"Resume Game",
						"Start With",
						numero3,
						"Credits",
						"Start With Credits & Random Ships",
						"Flower Domination",
						"Capture The Flower",
						"Eat The Mushroom",
						"Settings",
						"Exit Menu"};

	glBlendFunc(GL_ONE,GL_ONE_MINUS_SRC_ALPHA);	
//	printf("&=%i\n",'&');
	tempkoko=2;
	glPushMatrix();
	//glTranslatef(-0.2,-0.23,0);
	//glScalef(1.25,0.42,1);
	glColor4f(0,0,0,0.6);
	glBegin(GL_QUADS);
		glNormal3f(0,0,1.0f);
		glTexCoord2f(0.0f, 0.0f); glVertex3f(-2.6f, -1.20f, 10);
		glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.3f, -1.20f, 10);
		glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.3f,  0.55f, 10);
		glTexCoord2f(0.0f, 1.0f); glVertex3f(-2.6f,  0.55f, 10);
	glEnd();
	glPopMatrix();
	glBlendFunc(GL_ONE,GL_ONE_MINUS_SRC_COLOR);	

	glTranslatef(0.1,0.1,0.01);
	glColor4f(1,0,0,1);
	//gl_println( -8,  8, -8,vekotin[0]);
	gl_println(-25,  4,-40,vekotin[1]);
	gl_println(-25,  2,-40,vekotin[2]);
	gl_println(-25,  0,-40,vekotin[3]);
	gl_println(-12,  0,-40,vekotin[4]);
	gl_println( -5,  0,-40,vekotin[5]);
	gl_println(-25, -2,-40,vekotin[6]);
	gl_println(-25, -4,-40,vekotin[7]);
	gl_println(-25, -6,-40,vekotin[8]);
	gl_println(-25, -8,-40,vekotin[9]);
	gl_println(-25,-10,-40,vekotin[10]);
	gl_println(-25,-12,-40,vekotin[11]);


/*	glBindTexture(GL_TEXTURE_2D, texture[18]);				
	glColor4f(1.0f,1.0f,1.0f,1.0f);		
	glPushMatrix();
	glTranslatef(-7.5,1.0f-0.55*selection,0);
	tempkoko=0.6;
	glBegin(GL_QUADS);
		glNormal3f(0,0,1.0f);
		glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*tempkoko, -1.0f*tempkoko, 0);
		glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*tempkoko, -1.0f*tempkoko, 0);
		glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*tempkoko,  1.0f*tempkoko, 0);
		glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*tempkoko,  1.0f*tempkoko, 0);
	glEnd();
	glPopMatrix();
*/
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
		glTranslatef(-7.8,1.0f-0.55*selection,0);
		glRotatef(nuolirot,1,0,0);
		glScalef(0.2,0.3,0.3);
		glCallList(NUOLI);
		nuolirot+=2.5;
		if(nuolirot>180)nuolirot-=360;
	glPopMatrix();

	glEnable(GL_BLEND);
	SDL_GL_SwapBuffers();
//	delete[] vekotin;
}

