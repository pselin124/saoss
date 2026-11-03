#include "main.h"
#include "scene.h"
#include "glapi.h"
#include "net.h"

void playerMenu(){
	int i;
	bool menuready=false;
	int selection=0;
	int selectionrivi=0;
	bool quit=false;
	for(i=0;i<players;i++){
		player[i].pfire=false;
		player[i].palas=false;
		player[i].pright=false;
		player[i].pleft=false;
	}
	entterpressed=false;
	int selectionmax=players;
	while(!quit){
		//itse menu
		while(!menuready){
			processEvents();
			nuolirot+=2.5;
			if(nuolirot>180)nuolirot-=360;
			if(entterpressed){
				entterpressed=false;
				menuready=true;
			}else if(player[0].pleft || player[0].pright){
				if(selectionrivi==0) selectionrivi=1;
				else selectionrivi=0;
				if(selectionrivi==1 && selection==selectionmax)selection--;
				player[0].pleft=false;
				player[0].pright=false;
			}else if(player[0].pylos){
				if(selection>0) selection--;
				else selection=selectionmax;
				player[0].pylos=false;
			}else if(player[0].palas){
				if(selection<selectionmax) selection++;
				else selection=0;
				player[0].palas=false;
				if(selection==selectionmax && selectionrivi==1)selection=0;
			}else if(escpressed){
				selection=selectionmax;
				selectionrivi=0;
				menuready=true;
				escpressed=false;
			}
			if(selectionrivi==1 && selection==selectionmax) selection--;
			do{
				currenttime=SDL_GetTicks()-starttime;
				framecounter=calculatefps*currenttime/1000.0;
				if(oldframecounter==framecounter) SDL_Delay(2);
			}	while (oldframecounter==framecounter);
			oldframecounter=framecounter;
			drawPlayerMenu(selection,selectionrivi);
			updateAudio();
		}
		menuready=false;
		if(selection==selectionmax && selectionrivi==0){
			for(i=0;i<players;i++){
//				player[i].tiimi=sets->team[i];
				player[i].ihmispelaaja=sets->ihminen[i];
			}
			quit=true;
		}else if(selectionrivi==0){
			if(selection<=3 && selection<players){
				if(sets->ihminen[selection]==true)
					sets->ihminen[selection]=false;
				else sets->ihminen[selection]=true;
			}
		}else if(selectionrivi==1 && selection<selectionmax){
			if(player[selection].tiimi>=MAXNETPLAYERS-1){
				player[selection].tiimi=0;
			}else player[selection].tiimi++;
		}

	}
}

void drawPlayerMenu(int selection, int selectionrivi){
	glDisable(GL_LIGHTING);
	glEnable(GL_BLEND);
	glBlendFunc(GL_ONE,GL_ONE_MINUS_SRC_COLOR);	
	glDisable(GL_DEPTH_TEST);
	glEnable(GL_TEXTURE_2D);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);			
	gluPerspective(45.0f,(GLfloat)screenwidth/(GLfloat)screenheight,1.0f,100.0f);
	glLoadIdentity();							
	glMatrixMode(GL_MODELVIEW);

	gluLookAt(0,0,15,
	0,0,0,
	0,1,0);

	glColor4f(1.0f,1.0f,1.0f,1.0f);		
	glBindTexture(GL_TEXTURE_2D, texture[30]);				
	float tempkoko=3.0f/4.0f;
//	float tempkoko=(float)screenheight/(float)screenwidth;
	//float tempkoko=0.6f;
	glBegin(GL_QUADS);
		glNormal3f(0,0,1.0f);
		glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f, -1.0f*tempkoko, 13.3);
		glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f, -1.0f*tempkoko, 13.3);
		glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f,  1.0f*tempkoko, 13.3);
		glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f,  1.0f*tempkoko, 13.3);
	glEnd();


/*	glBindTexture(GL_TEXTURE_2D, texture[18]);				
	glColor4f(1,0,0,1);
	glPushMatrix();

	glTranslatef(-6.5f+5*selectionrivi,3.80-selection*0.54667,0);

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




	char *vekotin[]={	"Player Setup",
						"ai attacks ai:",
						"previous menu",
						"on",
						"off",
						"Player",
						":",
						"Human",
						"CPU",
						"Team"};

	//player setup
	glColor4f(1,0,0,1);
	gl_println( -11,  8, -8,vekotin[0]);

/*	//aimodet
	gl_println(-20, 14,-40,vekotin[1]);
	if(sets->aikohteet) gl_println( 8,  14,-40,vekotin[3]);
	else gl_println( 8,  14,-40,vekotin[4]);*/

	//previous menu
	gl_println(-20, 14-(players)*2,-40,vekotin[2]);
	
	//players, cpu/human
	int i;
//	if(players<10){
		for(i=0;i<players;i++){
			glColor4f(1,0,0,1);
			gl_println(-20, 14-(i)*2,-40,vekotin[5]); //player
			char numero1[10];
			itoa(i+1,numero1,10);	
			gl_println(-14, 14-(i)*2,-40,numero1);		//i
			gl_println(-11, 14-(i)*2,-40,vekotin[6]);	//:
			if(sets->ihminen[i]==true){
				glColor4f(0,1,0,1);
				gl_println(-10, 14-(i)*2,-40,vekotin[7]);
			}
			else{										//cpu tai human
				glColor4f(0,0,1,1);
				gl_println(-10, 14-(i)*2,-40,vekotin[8]);
			}
		}
//	}

	//players, team
//	if(players<10){
		for(i=0;i<players;i++){
			glColor4f(1,0,0,1);
			gl_println(0, 14-(i)*2,-40,vekotin[5]); //player
			char numero1[10];
			itoa(i+1,numero1,10);	
			gl_println( 6, 14-(i)*2,-40,numero1);		//i
			valitseVari(player[i].tiimi);
			gl_println( 9, 14-(i)*2,-40,vekotin[9]);  //team
			gl_println(15, 14-(i)*2,-40,vekotin[6]);	//:
			itoa(player[i].tiimi+1,numero1,10);	
			gl_println(16, 14-(i)*2,-40,numero1);	//:
		}
//	}


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
		glTranslatef(-6.7f+5.5*selectionrivi,3.80-selection*0.54667,0);
		glRotatef(nuolirot,1,0,0);
		glScalef(0.3,0.3,0.3);
		glCallList(NUOLI);
	glPopMatrix();



	glEnable(GL_BLEND);
	SDL_GL_SwapBuffers();
}

void asetusmenu(){
	int i;
	double temp3;
	bool menuready=false;
	int selection=0;
	bool quit=false;
	int tempwidth=sets->width;
	int tempheight=sets->height;
	int tempfull=sets->fullscreen;
	int tempai=sets->ai;
	int tempmus=sets->musics;
	int tempef=sets->efects;
	int tempcr=sets->credits;
	int tempfps=sets->fps;
	int tmv=sets->musicvolume;
	int tsv=sets->soundvolume;
	int tp=players;
	int resovaihtu = 0;
	for(i=0;i<players;i++){
		player[i].pfire=false;
	}
	int selectionmax=13;
	while(!quit){
		//itse menu
		while(!menuready){
			processEvents();
			nuolirot+=2.5;
			if(nuolirot>180)nuolirot-=360;
			if(entterpressed){
				entterpressed=false;
				menuready=true;
				player[0].pright=true;
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
				if(oldframecounter==framecounter) SDL_Delay(2);
			}	while (oldframecounter==framecounter);
			oldframecounter=framecounter;
			drawAsetusmenu(selection,tempwidth,tempheight,tempfull,tempai, 
						   tempmus,tempef,tempcr,tempfps, tmv, tsv, tp);
			updateAudio();
		}
		menuready=false;
		const SDL_VideoInfo *info;
		SDL_Surface *surface;
		int screenbpp;
		switch(selection){
		case 0:
			if(player[0].pright){
				modenumber--;
				if(modenumber>=0 && modes[modenumber]){
						tempwidth=modes[modenumber]->w;
						tempheight=modes[modenumber]->h;
				}else{
						modenumber=0;
						while(modes[modenumber]) modenumber++;
						modenumber--;
						tempwidth=modes[modenumber]->w;
						tempheight=modes[modenumber]->h;
				}
			}else{

				modenumber++;
				if(modes[modenumber]){
						tempwidth=modes[modenumber]->w;
						tempheight=modes[modenumber]->h;
				}else{
						modenumber=0;
						tempwidth=modes[modenumber]->w;
						tempheight=modes[modenumber]->h;
				}
				
				
			}

			break;
		case 1:
			if(tempfull!=0) tempfull=0;
			else tempfull=1;
			break;
		case 2:
			if(player[0].pright){
				if(tp==maxplayers) tp=2;
				else tp++;
			}else{
				if(tp==2) tp=maxplayers;
				else tp--;
			}
			break;
		case 3:
			if(tempmus!=0) tempmus=0;
			else tempmus=1;
			break;
			
		case 4:
			if(tempef!=0) tempef=0;
			else tempef=1;
			break;
		case 5:
			if(player[0].pright){
				tempcr+=25;
				if(tempcr>1000) tempcr=25;
			}else{
				tempcr-=25;
				if(tempcr<25) tempcr=1000;
			}
			break;
		case 6:
			if(player[0].pright){
				if(tempfps>=100) tempfps+=10;
				else tempfps+=5;
				if(tempfps>200) tempfps=50;
			}else{
				if(tempfps>=110) tempfps-=10;
				else tempfps-=5;
				if(tempfps<50)tempfps=200;
			}
			break;
		case 7:
			if(player[0].pright){
				if(player[0].pfire || player[1].pfire) tmv++;
				else tmv+=5;
				if(tmv>100) tmv=0;
			}else{
				if(player[0].pfire || player[1].pfire) tmv--;
				else tmv-=5;
				if(tmv<0) tmv=100;
			}
			sets->musicvolume=tmv;
			if(count>0 && musiikkipaalla) backgroundsong->setVolume(tmv/100.0f);
			break;
		case 8:
			if(player[0].pright){
				if(player[0].pfire || player[1].pfire) tsv++;
				else tsv+=5;
				if(tsv>100) tsv=0;
			}else{
				if(player[0].pfire || player[1].pfire) tsv--;
				else tsv-=5;
				if(tsv<0) tsv=100;
			}
			sets->soundvolume=tsv;
			break;
		case 9:
			if(tp!=players){
//				delete[] player;
				sets->playermaara=tp;
				players=tp;
//				player=new Player[tp];
				for(i=0;i<tp;i++){
					temp3=random(1000);
//					player[i].asetaAlus(alus[1]);
//					player[i].uudestaan(alus[(int)(temp3/1000.0f*(double)aluksia)]);
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
					player[i].tiimi=sets->team[i];
					player[i].ihmispelaaja=sets->ihminen[i];
				}
			}
			playerMenu();
			for(i=0;i<tp;i++){
					sets->team[i]=player[i].tiimi;
			}
			break;
		case 10:
			if(sets->cammode==0) sets->cammode=1;
			else sets->cammode=0;
			break;
		case 11:
			if(player[0].pright){
				if(sets->detail>sets->maxdetail-1) sets->detail=0;
				else sets->detail++;
			}else{
				if(sets->detail<1) sets->detail=sets->maxdetail;
				else sets->detail--;
			}
			sirpaleita=1000+sets->detail*500;
			alkusirpaleita=sirpaleita/12.5f;
			break;
		case 12:
			resovaihtu = 0;
			if(sets->width!=tempwidth) resovaihtu++;
			if(sets->height!=tempheight) resovaihtu++;
			if(sets->fullscreen!=tempfull) resovaihtu++;
			sets->width=tempwidth;
			sets->height=tempheight;
			sets->ai=tempai;
			sets->credits=tempcr;
			sets->fps=tempfps;
			sets->musics=tempmus;
			sets->efects=tempef;
			sets->musicvolume=tmv;
			sets->soundvolume=tsv;
			screenwidth=tempwidth;
			screenheight=tempheight;
			sirpaleita=1000+sets->detail*500;
			alkusirpaleita=sirpaleita/12.5f;
			sets->playermaara=tp;
			fullcredits=tempcr;
			if(tp!=players){
//				delete[] player;
				players=tp;
//				player=new Player[tp];
				for(i=0;i<tp;i++){
					temp3=random(1000);
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
					player[i].tiimi=sets->team[i];
					player[i].ihmispelaaja=sets->ihminen[i];
				}
			}
/*			for(i=0;i<players;i++){
				player[i].tiimi=sets->team[i];
				player[i].ihmispelaaja=sets->ihminen[i];
			}*/
			calculatefps=tempfps;
			ai=tempai;
			info=SDL_GetVideoInfo();
			if (!info){
				printf("Could not get video info with SDL: %s.\n",SDL_GetError());
				exitProgram(2);
			}
			//screenbpp=info->vfmt->BitsPerPixel;
//			screenwidth=surface->w;
//			screenheight=surface->h;
//			if(tempfull!=sets->fullscreen){
			if(resovaihtu){
				sets->fullscreen=tempfull;
				if(tempfull>0){
					if (!SDL_SetVideoMode(screenwidth,screenheight,screenbpp,SDL_RESIZABLE | SDL_OPENGL | SDL_FULLSCREEN)){
						fprintf(stderr,"Couldn't set %i*%i*%i opengl video mode: %s\n",screenwidth,screenheight,screenbpp,SDL_GetError());
						exitProgram(-1);
					}
				}else if (!SDL_SetVideoMode(screenwidth,screenheight,screenbpp,SDL_RESIZABLE | SDL_OPENGL)){
						fprintf(stderr,"Couldn't set %i*%i*%i opengl video mode: %s\n",screenwidth,screenheight,screenbpp,SDL_GetError());
						exitProgram(-1);
				}
				setupOpengl(tempwidth, tempheight);
				surface=SDL_GetVideoSurface();
				screenwidth=surface->w;
				screenheight=surface->h;
				tempwidth=surface->w;
				tempheight=surface->h;
			}
			
			printf("\n%i\n",musiikkipaalla);
			if(!musiikkipaalla && tempmus!=0) {
				musiikkipaalla=true;
				int gah=random(count);
				if(count>0){
					backgroundsong=new Sound(names[gah]);
					backgroundsong->play();
					backgroundsong->setStopCallback(test);
					backgroundsong->setVolume(sets->musicvolume/100.0f);
				}
			} else if(tempmus==0 && musiikkipaalla){
				musiikkipaalla=false;
				if(count>0)backgroundsong->stop();
			}
			printf("möö\n");
			if(!tempef) efektitpaalla=false;
			else efektitpaalla=true;
			setSoundVolumes();	
			for(i=0;i<players;i++){
				if(player[i].ampumisaani!=NULL) delete player[i].ampumisaani;
				if(player[i].eampumisaani!=NULL) delete player[i].eampumisaani;
				if(player[i].osumisaani!=NULL) delete player[i].osumisaani;
				if(player[i].eosumisaani!=NULL) delete player[i].eosumisaani;
				player[i].tiimi=sets->team[i];

				if(player[i].alus.ampumisaani!=NULL) player[i].ampumisaani=new Sound(player[i].alus.ampumisaani);
				else player[i].ampumisaani=NULL;
				if(player[i].alus.eampumisaani!=NULL) player[i].eampumisaani=new Sound(player[i].alus.eampumisaani);
				else player[i].eampumisaani=NULL;
				if(player[i].alus.osumisaani!=NULL) player[i].osumisaani=new Sound(player[i].alus.osumisaani);
				else player[i].osumisaani=NULL;
				if(player[i].alus.eosumisaani!=NULL) player[i].eosumisaani=new Sound(player[i].alus.eosumisaani);
				else player[i].eosumisaani=NULL;
			}

			delete sirpale;
			sirpale=new Sirpale[sirpaleita];
			for(i=0;i<MAXNETPLAYERS;i++){
				delete player[i].alkusirpale;
				player[i].alkusirpale=(Sirpale *) malloc(sizeof(Sirpale)*alkusirpaleita);
				memset(player[i].alkusirpale,0,sizeof(Sirpale)*alkusirpaleita);
			}
			if(sets->saveFile()!=1)
				printf("Saving settings not finished\n");
			setSoundVolumes();				
			break;
		case 13: //apply,save,exit
			quit=true;
			resovaihtu = 0;
			if(sets->width!=tempwidth) resovaihtu++;
			if(sets->height!=tempheight) resovaihtu++;
			if(sets->fullscreen!=tempfull) resovaihtu++;
			sets->width=tempwidth;
			sets->height=tempheight;
			sets->ai=tempai;
			sets->credits=tempcr;
			sets->fps=tempfps;
			sets->musics=tempmus;
			sets->efects=tempef;
			sets->musicvolume=tmv;
			sets->soundvolume=tsv;
			sets->fullscreen=tempfull;
			screenwidth=tempwidth;
			screenheight=tempheight;
			sirpaleita=1000+sets->detail*500;
			alkusirpaleita=sirpaleita/12.5f;
			sets->playermaara=tp;
			fullcredits=tempcr;
			if(tp!=players){
//				delete[] player;
				players=tp;
//				player=new Player[tp];
				for(i=0;i<tp;i++){
					temp3=random(1000);
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
					player[i].tiimi=sets->team[i];
					player[i].ihmispelaaja=sets->ihminen[i];
				}
			}
/*			for(i=0;i<players;i++){
				player[i].tiimi=sets->team[i];
				player[i].ihmispelaaja=sets->ihminen[i];
			}*/
			calculatefps=tempfps;
			ai=tempai;
			info=SDL_GetVideoInfo();
			if (!info){
				printf("Could not get video info with SDL: %s.\n",SDL_GetError());
				exitProgram(2);
			}
			screenbpp=info->vfmt->BitsPerPixel;
//			screenwidth=surface->w;
//			screenheight=surface->h;
//			if(tempfull!=sets->fullscreen){
			sets->fullscreen=tempfull;
			if(resovaihtu){
				if(tempfull>0){
//					if (!SDL_SetVideoMode(screenwidth,screenheight,screenbpp,SDL_RESIZABLE | SDL_OPENGL | SDL_FULLSCREEN)){
					if (!SDL_SetVideoMode(screenwidth,screenheight,screenbpp,SDL_RESIZABLE | SDL_OPENGL | SDL_FULLSCREEN
						  | SDL_ANYFORMAT)){
						fprintf(stderr,"Couldn't set %i*%i*%i opengl video mode: %s\n",screenwidth,screenheight,screenbpp,SDL_GetError());
						exitProgram(-1);
					}
				}else if (!SDL_SetVideoMode(screenwidth,screenheight,screenbpp,SDL_RESIZABLE | SDL_OPENGL)){
						fprintf(stderr,"Couldn't set %i*%i*%i opengl video mode: %s\n",screenwidth,screenheight,screenbpp,SDL_GetError());
						exitProgram(-1);
				}
				setupOpengl(tempwidth, tempheight);
				surface=SDL_GetVideoSurface();
				screenwidth=surface->w;
				screenheight=surface->h;
				tempwidth=surface->w;
				tempheight=surface->h;
			}
			
			printf("\n%i\n",musiikkipaalla);
			if(!musiikkipaalla && tempmus!=0) {
				musiikkipaalla=true;
				int gah=random(count);
				if(count>0){
					backgroundsong=new Sound(names[gah]);
					backgroundsong->play();
					backgroundsong->setStopCallback(test);
					backgroundsong->setVolume(sets->musicvolume/100.0f);
				}
			} else if(tempmus==0 && musiikkipaalla){
				musiikkipaalla=false;
				if(count>0)backgroundsong->stop();
			}
			printf("möö\n");
			if(!tempef) efektitpaalla=false;
			else efektitpaalla=true;
			setSoundVolumes();	
			delete sirpale;
			sirpale=new Sirpale[sirpaleita];
			for(i=0;i<players;i++){
				if(player[i].ampumisaani!=NULL) delete player[i].ampumisaani;
				if(player[i].eampumisaani!=NULL) delete player[i].eampumisaani;
				if(player[i].osumisaani!=NULL) delete player[i].osumisaani;
				if(player[i].eosumisaani!=NULL) delete player[i].eosumisaani;
				player[i].tiimi=sets->team[i];

				if(player[i].alus.ampumisaani!=NULL) player[i].ampumisaani=new Sound(player[i].alus.ampumisaani);
				else player[i].ampumisaani=NULL;
				if(player[i].alus.eampumisaani!=NULL) player[i].eampumisaani=new Sound(player[i].alus.eampumisaani);
				else player[i].eampumisaani=NULL;
				if(player[i].alus.osumisaani!=NULL) player[i].osumisaani=new Sound(player[i].alus.osumisaani);
				else player[i].osumisaani=NULL;
				if(player[i].alus.eosumisaani!=NULL) player[i].eosumisaani=new Sound(player[i].alus.eosumisaani);
				else player[i].eosumisaani=NULL;
			}

			for(i=0;i<MAXNETPLAYERS;i++){
				delete player[i].alkusirpale;
				player[i].alkusirpale=(Sirpale *) malloc(sizeof(Sirpale)*alkusirpaleita);
				if(player[i].alkusirpale!=NULL)
					memset(player[i].alkusirpale,0,sizeof(Sirpale)*alkusirpaleita);
				else{
					printf("muistibugi\n");
					exitProgram(-1);
				}
			}

/*			sets->width=tempwidth;
			sets->height=tempheight;
			sets->fullscreen=tempfull;
			sets->ai=tempai;
			sets->credits=tempcr;
			sets->fps=tempfps;
			sets->musics=tempmus;
			sets->efects=tempef;
			sets->musicvolume=tmv;
			sets->soundvolume=tsv;
			sets->playermaara=tp;*/
			if(sets->saveFile()!=1)
				printf("Saving settings not finished\n");
			setSoundVolumes();				
			break;
		}
		player[0].pleft=false;
		player[0].pright=false;
	}
}



void drawAsetusmenu(int selection, 
						   int twidht, 
						   int theight, 
						   int tfull, 
						   int tai,
						   int tmus,
						   int tef,
						   int tcr,
						   int tfps,
						   int tmv,
						   int tsv,
						   int tp){
//void drawAsetusmenu(int selection, int twidht, int theight, int tfull, int tai){
	glDisable(GL_LIGHTING);
	glEnable(GL_BLEND);
	glBlendFunc(GL_ONE,GL_ONE_MINUS_SRC_COLOR);	
	glDisable(GL_DEPTH_TEST);
	glEnable(GL_TEXTURE_2D);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);			
	gluPerspective(45.0f,(GLfloat)screenwidth/(GLfloat)screenheight,1.0f,100.0f);
	glLoadIdentity();							
	glMatrixMode(GL_MODELVIEW);

	gluLookAt(0,0,15,
	0,0,0,
	0,1,0);

	glColor4f(1.0f,1.0f,1.0f,1.0f);		
	glBindTexture(GL_TEXTURE_2D, texture[30]);				
	float tempkoko=3.0f/4.0f;
//	float tempkoko=(float)screenheight/(float)screenwidth;
	//float tempkoko=0.6f;
	glBegin(GL_QUADS);
		glNormal3f(0,0,1.0f);
		glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f, -1.0f*tempkoko, 13.3);
		glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f, -1.0f*tempkoko, 13.3);
		glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f,  1.0f*tempkoko, 13.3);
		glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f,  1.0f*tempkoko, 13.3);
	glEnd();


	/*glBindTexture(GL_TEXTURE_2D, texture[18]);				
	glColor4f(1,1,1,1);
	glPushMatrix();
	glTranslatef(-6.5f,3.80-selection*0.54667,0);
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
	char numero1[10];
	char numero2[10];
	char numero3[10];
	char numero4[10];
	char numero5[10];
	char numero6[10];
	char numero7[10];
	char numero8[10];
	itoa(twidht,  numero1, 10);
	itoa(theight, numero2, 10);
	itoa(tcr,	  numero3, 10);
	itoa(tfps,	  numero4, 10);
	itoa(tmv,	  numero5, 10);
	itoa(tsv,	  numero6, 10);
	itoa(tp,	  numero7, 10);
	itoa(sets->detail,	  numero8, 10);

	char *vekotin2[]={	numero1,
						"x",
						numero2,
						"on",
						"off",
						numero3,
						numero4,
						numero5,
						numero6,
						numero7,
						"3d",
						"2d",
						numero8};

	char *vekotin[]={	"Settings",
						"Resolution:",
						"Fullscreen:",
						"Players:",
						"Musics:",
						"Sound Effects:",
						"Credits:",
						"Fps:",
						"Music Volume:",
						"Sound Volume:",
						"Player Setup",
						"Camera Mode:",//11
						"Detail:",
						"Apply and Save Settings",//11
						"Apply, Save and Exit Menu"};
/*						numero3,
						"points",
						"quit"};*/
	glColor4f(1,0,0,1);

	gl_println( -8,  8, -8,vekotin[0]);
	gl_println(-20, 14,-40,vekotin[1]);
	gl_println(-20, 12,-40,vekotin[2]);
	gl_println(-20, 10,-40,vekotin[3]);
	gl_println(-20,  8,-40,vekotin[4]);
	gl_println(-20,  6,-40,vekotin[5]);
	gl_println(-20,  4,-40,vekotin[6]);
	gl_println(-20,  2,-40,vekotin[7]);
	gl_println(-20,  0,-40,vekotin[8]);
	gl_println(-20, -2,-40,vekotin[9]);
	gl_println(-20, -4,-40,vekotin[10]);
	gl_println(-20, -6,-40,vekotin[11]);
	gl_println(-20, -8,-40,vekotin[12]);
	gl_println(-20,-10,-40,vekotin[13]);
	gl_println(-20,-12,-40,vekotin[14]);

	//reso
	gl_println( -4, 14,-40,vekotin2[0]);
	gl_println(  2, 14,-40,vekotin2[1]);
	gl_println(  5, 14,-40,vekotin2[2]);

	if(tfull!=0) gl_println( -4,  12,-40,vekotin2[3]);
	else gl_println( -4,  12,-40,vekotin2[4]);

//	if(tai!=0) gl_println( 8,  10,-40,vekotin2[3]);
//	else gl_println( 8,  10,-40,vekotin2[4]);
	gl_println(  -4, 10,-40,vekotin2[9]);

	if(tmus!=0) gl_println( -4,  8,-40,vekotin2[3]);
	else gl_println( -4,  8,-40,vekotin2[4]);

	if(tef!=0) gl_println( -4,  6,-40,vekotin2[3]);
	else gl_println( -4,  6,-40,vekotin2[4]);
	
	gl_println(  -4,  4,-40,vekotin2[5]);

	gl_println(  -4,  2,-40,vekotin2[6]);

	gl_println(  -4,  0,-40,vekotin2[7]);

	gl_println(  -4, -2,-40,vekotin2[8]);

	if(sets->cammode==0)
		gl_println(  -4,-6,-40,vekotin2[10]);
	else 
		gl_println(  -4,-6,-40,vekotin2[11]);

	//detail
  gl_println(  -4,-8, -40,vekotin2[12]);


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
		glTranslatef(-6.7f,3.80-selection*0.54667,0);
		glRotatef(nuolirot,1,0,0);
		glScalef(0.3,0.3,0.3);
		glCallList(NUOLI);
	glPopMatrix();

	glEnable(GL_BLEND);
	SDL_GL_SwapBuffers();
}

bool saavalita;
bool joinpeli=false, hostpeli=false;
bool debugmode=false;


void mainmenu(){
	bool menuready=false;
	int selection=0;
	bool pistevoitto=false;
	int selectionmax=4;
	//debugmode=true;
	if(debugmode)selectionmax++;
	saavalita=true;
//	printf("efects=%i\n",sets->efects);
	while(1){
		//itse menu
		while(!menuready){
			processEvents();
			nuolirot+=2.5;
			if(nuolirot>180)nuolirot-=360;
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
				if(oldframecounter==framecounter) SDL_Delay(2);
			}	while (oldframecounter==framecounter);
			oldframecounter=framecounter;
			drawmenu(selection);
			updateAudio();
		}
		menuready=false;
		//game ilman alusten uudelleen laittoa (resume)
		if(selection==0){
			onePlayerGameMenu();
		}
		else if(selection==1) {
			hostMultiplayer();
		}
		else if(selection==2){
			joinMultiplayer();
		}
		else if(selection==3){
			asetusmenu();
		}
		else if(selection==selectionmax){
			break;
			//exitProgram(0);
		}
		else if(selection==4){
			printf("debug\n");
			debugPiirtomenu();
		}

		escpressed=false;
		entterpressed=false;
		player[0].pright=false;
		player[0].pleft=false;
		player[0].palas=false;
		player[0].pylos=false;
		player[0].pfire=false;


	}

}

void debugPiirtomenu(){
	bool menuready=false;
	int selection=0;
	entterpressed=false;
	escpressed=false;
	bool pistevoitto=false;
	int selectionmax=7;
	if(debugmode)selectionmax++;
	saavalita=true;
	int dalus=0;

	while(!menuready){
		processEvents();
		joinpeli=false; hostpeli=false;
		if(entterpressed || escpressed){
			entterpressed=false;
			escpressed=false;
			menuready=true;
		}else if(player[0].pylos){
			dalus++;
			if(dalus>=aluksia)dalus=0;
			player[0].pylos=false;
		}else if(player[0].palas){
			dalus--;
			if(dalus<0)dalus=aluksia-1;
			player[0].palas=false;
		}
		do{
			currenttime=SDL_GetTicks()-starttime;
			framecounter=calculatefps*currenttime/1000.0;
			if(oldframecounter==framecounter) SDL_Delay(5);
		}	while (oldframecounter==framecounter);
		updateAudio();
		drawDebugMenu(dalus);
	}
}

void drawDebugMenu(int dalus){
	glDisable(GL_LIGHTING);
	glDisable(GL_BLEND);
	glEnable(GL_TEXTURE_2D);
	gluPerspective(45.0f,(GLfloat)screenwidth/(GLfloat)screenheight,1.0f,100.0f);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);			
	glLoadIdentity();							
	glMatrixMode(GL_MODELVIEW);

	GLfloat lightposition[4] = {0, 0, 0, 1};
	glLightfv(GL_LIGHT0,GL_POSITION,lightposition);

	gluLookAt(0,0,10,
	0,0,0,
	0,1,0);
	glRotatef(-60, 1, 0, 0);
	glRotatef(framecounter, 0, 0, 1);
	
	//player[0].suojapaalla=true;
	alus[dalus].piirra(0,0, 0, 90, 0, 0);


	glEnable(GL_BLEND);
//	glEnable(GL_LIGHTING);
//	glDisable(GL_TEXTURE_2D);
	SDL_GL_SwapBuffers();
}


void drawmenu(int selection){
	glDisable(GL_LIGHTING);
	glEnable(GL_BLEND);
	glBlendFunc(GL_ONE,GL_ONE_MINUS_SRC_COLOR);	
	glDisable(GL_DEPTH_TEST);
	glEnable(GL_TEXTURE_2D);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);			
//	gluPerspective(45.0f,(GLfloat)screenwidth/(GLfloat)screenheight,0.5f,80.0f);
	gluPerspective(45.0f,(GLfloat)4.0f/(GLfloat)3.0f,0.5f,80.0f);
	glLoadIdentity();							
	glMatrixMode(GL_MODELVIEW);


	glColor4f(1.0f,1.0f,1.0f,1.0f);		
	glBindTexture(GL_TEXTURE_2D, texture[31]);	//27			
	//float tempkoko=(float)screenheight/(float)screenwidth;
	float tempkoko=3.0f/4.0f;
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

	char *vekotin[]={	"main menu",
						"Local Game",
						"Host Multiplayer",
						"Join Multiplayer",
						"Settings",
						"Quit"};

	glBlendFunc(GL_ONE,GL_ONE_MINUS_SRC_ALPHA);	

	tempkoko=2;
	glPushMatrix();
//	glTranslatef(-0.2,-0.23,0);
//	glScalef(1.25,0.42,1);
	glColor4f(0,0,0,0.6);
	glBegin(GL_QUADS);
		glNormal3f(0,0,1.0f);
		glTexCoord2f(0.0f, 0.0f); glVertex3f(-2.0f, -0.5f, 10);
		glTexCoord2f(1.0f, 0.0f); glVertex3f( -0.1f, -0.5f, 10);
		glTexCoord2f(1.0f, 1.0f); glVertex3f( -0.1f,  0.5f, 10);
		glTexCoord2f(0.0f, 1.0f); glVertex3f(-2.0f,  0.5f, 10);

/*		glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*tempkoko, -1.0f*tempkoko, 10);
		glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*tempkoko, -1.0f*tempkoko, 10);
		glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*tempkoko,  1.0f*tempkoko, 10);
		glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*tempkoko,  1.0f*tempkoko, 10);*/
	glEnd();
	glPopMatrix();
	glBlendFunc(GL_ONE,GL_ONE_MINUS_SRC_COLOR);	

	glTranslatef(0.1,0.1,0.01);
	glColor4f(1,0,0,1);
	//gl_println( -8,  8, -8,vekotin[0]);
	gl_println(-20,  4,-40,vekotin[1]);
	gl_println(-20,  2,-40,vekotin[2]);
	gl_println(-20,  0,-40,vekotin[3]);
	gl_println(-20, -2,-40,vekotin[4]);
	gl_println(-20, -4,-40,vekotin[5]);


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
//glGetLightfv(GL_LIGHT0,GL_POSITION,lightposition);
//printf("%f,%f,%f,%f\n",lightposition[0],lightposition[1],lightposition[2],lightposition[3]);

  glPushMatrix();
		glEnable(GL_LIGHTING);
		glDisable(GL_BLEND);
		glEnable(GL_DEPTH_TEST);
		glDisable(GL_TEXTURE_2D);
		glTranslatef(-6.5,1.0f-0.55*selection,0);
		glRotatef(nuolirot,1,0,0);
		glScalef(0.22,0.3,0.3);
		glCallList(NUOLI);
	glPopMatrix();
	glPushMatrix();
		if(selection==0){
			glTranslatef(-1.9,1.0f-0.55*selection,0);
		}else if(selection==1){
			glTranslatef(-0.6,1.0f-0.55*selection,0);
		}else if(selection==2){
			glTranslatef( -0.8,1.0f-0.55*selection,0);
		}else if(selection==3){
			glTranslatef(-2.6,1.0f-0.55*selection,0);
		}else if(selection==4){
			glTranslatef(-3.9,1.0f-0.55*selection,0);
		}
		glRotatef(180,0,0,1);
		glRotatef(nuolirot,1,0,0);
		glScalef(0.22,0.3,0.3);
		glCallList(NUOLI);
	glPopMatrix();

	//glEnable(GL_BLEND);
	SDL_GL_SwapBuffers();
}


/*void gl_println(float x, float y, float z, char *vekotin){
	int i,pituus;
	for(i=0;i<1000;i++){
		if(vekotin[i]=='\0'){
			pituus=i;
			i=1000;
		}
	}
	double koko=2.0f;
//	pituus=1;
	for(i=0;i<pituus;i++){
		float x1, x2, y1, y2;
		if(vekotin[i]>='a' && vekotin[i]<'t'){
			x1=(float)(vekotin[i]-'a')*13.0f/256.0f+0.0f/256.0f;
			x2=(float)(vekotin[i]-'a')*13.0f/256.0f+13.0f/256.0f;
			y1=1.0f-18.0f/256.0f;
			y2=1.0f-2.0f/256.0f;
		}else if(vekotin[i]>='t' && vekotin[i]<='z'){
			x1=(float)(vekotin[i]-'t')*13.0f/256.0f;
			x2=(float)(vekotin[i]-'t')*13.0f/256.0f+12.0f/256.0f;
			y1=1.0f-39.0f/256.0f;
			y2=1.0f-23.0f/256.0f;
		}else if(vekotin[i]=='ä' || vekotin[i]==':'){
			x1=0;
			x2=14.0f/255.0f;
			y1=1.0f-66.0f/255.0f;
			y2=1.0f-54.0f/255.0f;
		}else if(vekotin[i]=='&'){
			x1=0;
			x2=13.0f/255.0f;
			y1=1.0f-78.0f/255.0f;
			y2=1.0f-66.0f/255.0f;
		}else if(vekotin[i]==','){
			x1=14.0f/255.0f;
			x2=28.0f/255.0f;
			y1=1.0f-78.0f/255.0f;
			y2=1.0f-66.0f/255.0f;
		}else if(vekotin[i]=='.'){
			x1=28.0f/255.0f;
			x2=42.0f/255.0f;
			y1=1.0f-78.0f/255.0f;
			y2=1.0f-66.0f/255.0f;
		}else if(vekotin[i]=='/'){
			x1=0.0f/255.0f;
			x2=14.0f/255.0f;
			y1=1.0f-98.0f/255.0f;
			y2=1.0f-82.0f/255.0f;
		}else if(vekotin[i]==' '){
			x1=0;
			x2=14.0f/255.0f;
			y1=1.0f-190.0f/256.0f;
			y2=1.0f-170.0f/256.0f;
		}else if(vekotin[i]=='.'){
			x1=0;
			x2=14.0f/255.0f;
			y1=1.0f-60.0f/256.0f;
			y2=1.0f-48.0f/256.0f;
		}else {
			x1=(float)(vekotin[i]-'0')*13.0f/256.0f;
			x2=(float)(vekotin[i]-'0')*13.0f/256.0f+13.0f/255.0f;
			y1=1.0f-129.0f/256.0f;
			y2=1.0f-114.0f/256.0f;

		}
		
		glBindTexture(GL_TEXTURE_2D, texture[10]);				
		glColor4f(1.0f,1.0f,1.0f,1.0f);		
		glPushMatrix();
		glTranslatef(x+i*koko, y, z);
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			glTexCoord2f(x1, 1.0f-y1); glVertex3f(-1.0f, -1.0f, 0);
			glTexCoord2f(x2, 1.0f-y1); glVertex3f( 1.0f, -1.0f, 0);
			glTexCoord2f(x2, 1.0f-y2); glVertex3f( 1.0f,  1.0f, 0);
			glTexCoord2f(x1, 1.0f-y2); glVertex3f(-1.0f,  1.0f, 0);
		glEnd();
		glPopMatrix();
	}
}
*/

/*void gl_println(float x, float y, float z, char *vekotin){
	int i,pituus;
	for(i=0;i<1000;i++){
		if(vekotin[i]=='\0'){
			pituus=i;
			i=1000;
		}
	}
	double koko=2.0f;
//	pituus=1;
	for(i=0;i<pituus;i++){
		float x1, x2, y1, y2;
		if(vekotin[i]>'A' && vekotin[i]<'Z') vekotin[i]-='A'-'a';
		if(vekotin[i]>='a' && vekotin[i]<'t'){
			x1=(float)(vekotin[i]-'a')*13.0f/256.0f+0.0f/256.0f;
			x2=(float)(vekotin[i]-'a')*13.0f/256.0f+13.0f/256.0f;
			y1=1.0f-18.0f/256.0f;
			y2=1.0f-2.0f/256.0f;
		}else if(vekotin[i]>='t' && vekotin[i]<='z'){
			x1=(float)(vekotin[i]-'t')*13.0f/256.0f;
			x2=(float)(vekotin[i]-'t')*13.0f/256.0f+12.0f/256.0f;
			y1=1.0f-39.0f/256.0f;
			y2=1.0f-23.0f/256.0f;
		}else if(vekotin[i]=='ä' || vekotin[i]==':'){
			x1=0;
			x2=14.0f/255.0f;
			y1=1.0f-66.0f/255.0f;
			y2=1.0f-54.0f/255.0f;
		}else if(vekotin[i]=='&'){
			x1=0;
			x2=13.0f/255.0f;
			y1=1.0f-78.0f/255.0f;
			y2=1.0f-66.0f/255.0f;
		}else if(vekotin[i]==','){
			x1=14.0f/255.0f;
			x2=28.0f/255.0f;
			y1=1.0f-78.0f/255.0f;
			y2=1.0f-66.0f/255.0f;
		}else if(vekotin[i]=='.'){
			x1=28.0f/255.0f;
			x2=42.0f/255.0f;
			y1=1.0f-78.0f/255.0f;
			y2=1.0f-66.0f/255.0f;
		}else if(vekotin[i]=='='){
			x1=14.0f/255.0f;
			x2=28.0f/255.0f;
			y1=1.0f-98.0f/255.0f;
			y2=1.0f-82.0f/255.0f;
		}else if(vekotin[i]=='-'){
			x1=28.0f/255.0f;
			x2=42.0f/255.0f;
			y1=1.0f-98.0f/255.0f;
			y2=1.0f-82.0f/255.0f;
		}else if(vekotin[i]=='/'){
			x1=0.0f/255.0f;
			x2=14.0f/255.0f;
			y1=1.0f-98.0f/255.0f;
			y2=1.0f-82.0f/255.0f;
		}else if(vekotin[i]==' '){
			x1=0;
			x2=14.0f/255.0f;
			y1=1.0f-190.0f/256.0f;
			y2=1.0f-170.0f/256.0f;
		}else if(vekotin[i]=='.'){
			x1=0;
			x2=14.0f/255.0f;
			y1=1.0f-60.0f/256.0f;
			y2=1.0f-48.0f/256.0f;
		}else {
			x1=(float)(vekotin[i]-'0')*13.0f/256.0f;
			x2=(float)(vekotin[i]-'0')*13.0f/256.0f+13.0f/255.0f;
			y1=1.0f-129.0f/256.0f;
			y2=1.0f-114.0f/256.0f;

		}
		
		glBindTexture(GL_TEXTURE_2D, texture[10]);				
//		glColor4f(1.0f,1.0f,1.0f,1.0f);		
		glPushMatrix();
		glTranslatef(x+i*koko, y, z);
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			glTexCoord2f(x1, 1.0f-y1); glVertex3f(-1.0f, -1.0f, 0);
			glTexCoord2f(x2, 1.0f-y1); glVertex3f( 1.0f, -1.0f, 0);
			glTexCoord2f(x2, 1.0f-y2); glVertex3f( 1.0f,  1.0f, 0);
			glTexCoord2f(x1, 1.0f-y2); glVertex3f(-1.0f,  1.0f, 0);
		glEnd();
		glPopMatrix();
	}
}
*/
float letterwidth[256] = {
	1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0,
	1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0,
	0.5, 0.2, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0.2, 0.45, 0.2, 0.5,
	0.6, 0.5, 0.6, 0.6, 0.65, 0.65, 0.6, 0.65, 0.6, 0.6, 0.2, 1.0, 1.0, 1.0, 1.0, 0.5,
	1.0, 0.7, 0.6, 0.7, 0.7, 0.65, 0.6, 0.7, 0.8, 0.6, 0.7, 0.7, 0.6, 0.9, 0.85, 0.8,
	0.6, 0.9, 0.7, 0.7, 0.7, 0.7, 0.7, 1.0, 0.8, 0.7, 0.8, 1.0, 1.0, 1.0, 1.0, 1.0,
	1.0, 0.6, 0.6, 0.6, 0.6, 0.6, 0.5, 0.6, 0.6, 0.2, 0.4, 0.6, 0.2, 0.8, 0.5, 0.55,
	0.55, 0.55, 0.5, 0.55, 0.55, 0.55, 0.6, 0.8, 0.6, 0.6, 0.6, 1.0, 1.0, 1.0, 1.0, 1.0,
	1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0,
	1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0,
	1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0,
	1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0,
	1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0,
	1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0,
	1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0,
	1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0
};

float gl_pituus(char *vekotin){
	int i,pituus;
	for(i=0;i<1000;i++){
		if(vekotin[i]=='\0'){
			pituus=i;
			i=1000;
		}
	}
	float rivinpituus=0;
	for(i=0;i<pituus;i++){
		rivinpituus+=letterwidth[vekotin[i]];
	}
	return rivinpituus;
}

void gl_println(float x, float y, float z, char *vekotin){
	int i,pituus;
	for(i=0;i<1000;i++){
		if(vekotin[i]=='\0'){
			pituus=i;
			i=1000;
		}
	}
	double koko=2.0f;
//	pituus=1;
	glBindTexture(GL_TEXTURE_2D, texture[28]);				
	glPushMatrix();
	glTranslatef(x, y, z);
	for(i=0;i<pituus;i++){
		float x1, x2, y1, y2;
		int xkohta, ykohta;
//		if(vekotin[i]>'A' && vekotin[i]<'Z') vekotin[i]-='A'-'a';
		char ch=vekotin[i];
		xkohta=ch&15;
		ykohta=(ch>>4);
		//printf("x=%i,y=%i,vekotin=%c\n",xkohta,ykohta,vekotin[i]);
		x1=(xkohta*64.0f+2.0f)/1024.0f;
		x2=((xkohta+1.0f)*64.0f-1.0f)/1024.0f;
		y2=(ykohta*64.0f+2.0f)/1024.0f;
		y1=((ykohta+1.0f)*64.0f-1.0f)/1024.0f;
		
//		glColor4f(1.0f,1.0f,1.0f,1.0f);		
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			glTexCoord2f(x1, y1); glVertex3f(-1.0f, -1.0f, 0);
			glTexCoord2f(x2, y1); glVertex3f( 1.0f, -1.0f, 0);
			glTexCoord2f(x2, y2); glVertex3f( 1.0f,  1.0f, 0);
			glTexCoord2f(x1, y2); glVertex3f(-1.0f,  1.0f, 0);
		glEnd();
		glTranslatef(koko*letterwidth[ch], 0, 0);
	}
	glPopMatrix();
}

void gl_println(float x, float y, float z, char *vekotin, float fonttikoko){
	int i,pituus;
	for(i=0;i<1000;i++){
		if(vekotin[i]=='\0'){
			pituus=i;
			i=1000;
		}
	}
	double koko=2.0f;
//	pituus=1;
	glBindTexture(GL_TEXTURE_2D, texture[28]);				
	glPushMatrix();
	glTranslatef(x, y, z);
	for(i=0;i<pituus;i++){
		float x1, x2, y1, y2;
		int xkohta, ykohta;
//		if(vekotin[i]>'A' && vekotin[i]<'Z') vekotin[i]-='A'-'a';
		char ch=vekotin[i];
		xkohta=ch&15;
		ykohta=(ch>>4);
		//printf("x=%i,y=%i,vekotin=%c\n",xkohta,ykohta,vekotin[i]);
		x1=(xkohta*64.0f+2.0f)/1024.0f;
		x2=((xkohta+1.0f)*64.0f-1.0f)/1024.0f;
		y2=(ykohta*64.0f+2.0f)/1024.0f;
		y1=((ykohta+1.0f)*64.0f-1.0f)/1024.0f;
		
//		glColor4f(1.0f,1.0f,1.0f,1.0f);		
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			glTexCoord2f(x1, y1); glVertex3f(-1.0f*fonttikoko, -1.0f*fonttikoko, 0);
			glTexCoord2f(x2, y1); glVertex3f( 1.0f*fonttikoko, -1.0f*fonttikoko, 0);
			glTexCoord2f(x2, y2); glVertex3f( 1.0f*fonttikoko,  1.0f*fonttikoko, 0);
			glTexCoord2f(x1, y2); glVertex3f(-1.0f*fonttikoko,  1.0f*fonttikoko, 0);
		glEnd();
		glTranslatef(koko*fonttikoko*letterwidth[ch], 0, 0);
	}
	glPopMatrix();
}

/*void gl_println(float x, float y, float z, char *vekotin, float fonttikoko){
	int i,pituus;
	for(i=0;i<1000;i++){
		if(vekotin[i]=='\0'){
			pituus=i;
			i=1000;
		}
	}
	double koko=2.0f;
//	pituus=1;
	for(i=0;i<pituus;i++){
		float x1, x2, y1, y2;
		if(vekotin[i]>='a' && vekotin[i]<'t'){
			x1=(float)(vekotin[i]-'a')*13.0f/256.0f+0.0f/256.0f;
			x2=(float)(vekotin[i]-'a')*13.0f/256.0f+13.0f/256.0f;
			y1=1.0f-18.0f/256.0f;
			y2=1.0f-2.0f/256.0f;
		}else if(vekotin[i]>='t' && vekotin[i]<='z'){
			x1=(float)(vekotin[i]-'t')*13.0f/256.0f;
			x2=(float)(vekotin[i]-'t')*13.0f/256.0f+12.0f/256.0f;
			y1=1.0f-39.0f/256.0f;
			y2=1.0f-23.0f/256.0f;
		}else if(vekotin[i]=='ä' || vekotin[i]==':'){
			x1=0;
			x2=14.0f/255.0f;
			y1=1.0f-66.0f/255.0f;
			y2=1.0f-54.0f/255.0f;
		}else if(vekotin[i]=='&'){
			x1=0;
			x2=13.0f/255.0f;
			y1=1.0f-78.0f/255.0f;
			y2=1.0f-66.0f/255.0f;
		}else if(vekotin[i]==','){
			x1=14.0f/255.0f;
			x2=28.0f/255.0f;
			y1=1.0f-78.0f/255.0f;
			y2=1.0f-66.0f/255.0f;
		}else if(vekotin[i]=='.'){
			x1=28.0f/255.0f;
			x2=42.0f/255.0f;
			y1=1.0f-78.0f/255.0f;
			y2=1.0f-66.0f/255.0f;
		}else if(vekotin[i]=='/'){
			x1=0.0f/255.0f;
			x2=14.0f/255.0f;
			y1=1.0f-98.0f/255.0f;
			y2=1.0f-82.0f/255.0f;
		}else if(vekotin[i]=='='){
			x1=14.0f/255.0f;
			x2=28.0f/255.0f;
			y1=1.0f-98.0f/255.0f;
			y2=1.0f-82.0f/255.0f;
		}else if(vekotin[i]=='-'){
			x1=28.0f/255.0f;
			x2=42.0f/255.0f;
			y1=1.0f-98.0f/255.0f;
			y2=1.0f-82.0f/255.0f;
		}else if(vekotin[i]==' '){
			x1=0;
			x2=14.0f/255.0f;
			y1=1.0f-190.0f/256.0f;
			y2=1.0f-170.0f/256.0f;
		}else if(vekotin[i]=='.'){
			x1=0;
			x2=14.0f/255.0f;
			y1=1.0f-60.0f/256.0f;
			y2=1.0f-48.0f/256.0f;
		}else {
			x1=(float)(vekotin[i]-'0')*13.0f/256.0f;
			x2=(float)(vekotin[i]-'0')*13.0f/256.0f+13.0f/255.0f;
			y1=1.0f-129.0f/256.0f;
			y2=1.0f-114.0f/256.0f;

		}
		
		glBindTexture(GL_TEXTURE_2D, texture[10]);				
//		glColor4f(1.0f,1.0f,1.0f,1.0f);		
		glPushMatrix();
		glTranslatef(x+i*koko*fonttikoko, y, z);
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			//glTexCoord2f(x1, y1); glVertex3f(-1.0f, -1.0f, 0);
			//glTexCoord2f(x2, y1); glVertex3f( 1.0f, -1.0f, 0);
			//glTexCoord2f(x2, y2); glVertex3f( 1.0f,  1.0f, 0);
			//glTexCoord2f(x1, y2); glVertex3f(-1.0f,  1.0f, 0);
			glTexCoord2f(x1, 1.0f-y1); glVertex3f(-1.0f*fonttikoko, -1.0f*fonttikoko, 0);
			glTexCoord2f(x2, 1.0f-y1); glVertex3f( 1.0f*fonttikoko, -1.0f*fonttikoko, 0);
			glTexCoord2f(x2, 1.0f-y2); glVertex3f( 1.0f*fonttikoko,  1.0f*fonttikoko, 0);
			glTexCoord2f(x1, 1.0f-y2); glVertex3f(-1.0f*fonttikoko,  1.0f*fonttikoko, 0);
		glEnd();
		glPopMatrix();
	}
}

*/
