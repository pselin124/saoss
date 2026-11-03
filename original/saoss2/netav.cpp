#include "main.h"
#include "net.h"
#include "netav.h"
#include "scene.h"

void drawNetAlusValinta(int numero, int alusnumero){
	glDisable(GL_LIGHTING);
	glDisable(GL_BLEND);
	glEnable(GL_TEXTURE_2D);
	glEnable(GL_DEPTH_TEST);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);			
//	glMatrixMode(GL_TEXTURE);
	glLoadIdentity();							

//	glScalef(1,-1,1);
	glMatrixMode(GL_MODELVIEW);
//	glMatrixMode(GL_MODELVIEW);

	GLfloat lightposition[4] = {0, 0, 0, 1};
	glLightfv(GL_LIGHT0,GL_POSITION,lightposition);

	gluLookAt(0,0,15,
	0,0,0,
	0,1,0);

	if(alusnumero!=aluksia){
		glPushMatrix();
		glRotatef(-60, 1, 0, 0);
		glTranslatef(-5, -2, -1.5);
		glRotatef(framecounter, 0, 0, 1);
		glScalef(0.5,0.5,0.5);
		alus[alusnumero].piirra(0, 0, 0, 90, 0, numero);
		glPopMatrix();
	}else{
		glPushMatrix();
		glRotatef(-45, 0, 0, 1);
		gl_println(-17, -22,-45,"random ship");
		glPopMatrix();
	}
	char numero1[10];
	char numero2[10];
	char numero3[10];
	char numero4[10];
	char numero5[10];
	char numero6[10];
	char numero7[10];
	char numero8[10];
	char numero9[10];
	char numero10[10];

		itoa( numero+1, numero1, 10);
		itoa( player[numero].pisteet, numero5, 10);
	if(alusnumero!=aluksia){
		itoa( alus[alusnumero].pistemaara, numero6, 10);
		itoa( alusnumero+1, numero8, 10);
	}else{
		sprintf(numero6,"x");
		sprintf(numero8,"x");
	}

	char *vekotin[]={"player",
					numero1,
					"choose your ship",
					"remaining credits:",
					numero5,
					"ship cost:",
					numero6,
					"ship number:",
					numero8,
					"name:"};

	//char *numero5=new char[3];
	if(alusnumero!=aluksia){
		itoa( alus[alusnumero].energia, numero3, 10);
		sprintf(numero4,"%.2f",alus[alusnumero].damage);
		sprintf(numero2,"%.2f",alus[alusnumero].edamage);
		sprintf(numero9,"%.2f",alus[alusnumero].kaantonopeus);
		sprintf(numero10,"%.2f",alus[alusnumero].paino);
	}else{
		itoa( alus[alusnumero].energia, numero3, 10);
		sprintf(numero3,"x");
		sprintf(numero4,"x");
		sprintf(numero2,"x");
		sprintf(numero9,"x");
		sprintf(numero10,"x");
	}

	double tempfloat;
	if(alusnumero!=aluksia){

		tempfloat=alus[alusnumero].nopeus*1000.0f;
		if(alusnumero==6 || alusnumero==18 || alusnumero==22)tempfloat/=100.0f;
		sprintf(numero7,"%.2f",tempfloat);
	}else{
		sprintf(numero7,"x");
	}

	char *vekotin2[]={	"energy:",
						numero3,
						"damage:",
						numero4,
						"special damage:",
						numero2,
						"speed:",
						numero7,
						"turning speed:",
						numero9,
						"weight:",
						numero10
	
	};
	glEnable(GL_BLEND);
	glDisable(GL_DEPTH_TEST);
//	glBlendFunc(GL_ALPHA,GL_ONE_MINUS_SRC_COLOR);	
//	glBlendFunc(GL_COLOR,GL_ONE_MINUS_SRC_COLOR);	
	glBlendFunc(GL_ONE,GL_ONE);	
	glColor4f(1,0,0,1);
	{
		//player x choose your ship
		gl_println(-5, 13,-25,vekotin[0]);
		gl_println(  3, 13,-25,vekotin[1]);
		gl_println(-10, 8,-25,vekotin[2]);

		//remaining credits
		gl_println(-25, 6,-40,vekotin[3]);
		gl_println( -5, 6,-40,vekotin[4]);

		//ship number
		gl_println(-25, 3.5,-40,vekotin[7]);
		gl_println(-10, 3.5,-40,vekotin[8]);

		//ship cost
		gl_println(-25, 1,-40,vekotin[5]);
		gl_println(-13, 1,-40,vekotin[6]);
//		gl_println(  5, 3,-40,vekotin[5]);
//		gl_println( 25, 3,-40,vekotin[6]);
	}
	{
		//arvot
		gl_println(-5, -5,-40,vekotin2[0]);
		gl_println(12, -5,-40,vekotin2[1]);

		gl_println(-5, -8,-40,vekotin2[2]);
		gl_println(12, -8,-40,vekotin2[3]);

		gl_println(-5, -11,-40,vekotin2[4]);
		gl_println(12, -11,-40,vekotin2[5]);
		
		gl_println(-5, -14,-40,vekotin2[6]);
		gl_println(12,  -14,-40,vekotin2[7]);

		gl_println(-5, -17,-40,vekotin2[8]);
		gl_println(12, -17,-40,vekotin2[9]);

		gl_println(-5, -20,-40,vekotin2[10]);
		gl_println(12, -20,-40,vekotin2[11]);
	}

	if(alusnumero!=aluksia){
		//gl_println( -20, -20,-40,vekotin[9]);
		//gl_println( -28, -20,-40,alus[alusnumero].nimi);
	}
/*
	{
		gl_println(-10, 13,-25,vekotin[0]);
		gl_println(  7, 13,-25,vekotin[1]);
		gl_println(-19, 8,-25,vekotin[2]);

		gl_println(-25, 6,-40,vekotin[3]);
		gl_println( 12, 6,-40,vekotin[4]);

		gl_println(-25, 3,-40,vekotin[7]);
		gl_println( -1, 3,-40,vekotin[8]);

		gl_println(-25, 1,-40,vekotin[5]);
		gl_println( -4, 1,-40,vekotin[6]);
//		gl_println(  5, 3,-40,vekotin[5]);
//		gl_println( 25, 3,-40,vekotin[6]);
	}
	{
		gl_println(-10, -5,-40,vekotin2[0]);
		gl_println(  4, -5,-40,vekotin2[1]);

		gl_println(-10, -8,-40,vekotin2[2]);
		gl_println(  4, -8,-40,vekotin2[3]);

		gl_println(-10, -11,-40,vekotin2[4]);
		gl_println( 21, -11,-40,vekotin2[5]);
		
		gl_println(-10, -14,-40,vekotin2[6]);
		gl_println( 3,  -14,-40,vekotin2[7]);

		gl_println(-10, -17,-40,vekotin2[8]);
		gl_println( 19, -17,-40,vekotin2[9]);

		gl_println(-10, -20,-40,vekotin2[10]);
		gl_println( 4, -20,-40,vekotin2[11]);
	}
*/
	if(alusnumero!=aluksia){
		//gl_println( -20, -20,-40,vekotin[9]);
		//gl_println( -28, -20,-40,alus[alusnumero].nimi);
	}



//	glEnable(GL_LIGHTING);
//	glDisable(GL_TEXTURE_2D);
	SDL_GL_SwapBuffers();
}

void netValitseAlus(int localplayer, int netplayer){
	player[localplayer].pfire=false;
	player[localplayer].palas=false;
	player[localplayer].pylos=false;
	player[localplayer].pleft=false;
	player[localplayer].pright=false;
	int randomluku=random(aluksia);
	int vastustaja;
	for(vastustaja=0;vastustaja<players;vastustaja++){
		if(player[vastustaja].tiimi!=player[netplayer].tiimi) break;
	}
	if(mpstatus[netplayer]==2){
		//jos vastustaja alus6..
		if(player[vastustaja].alus.aselaji==6){
			randomluku=random(100);
			if(randomluku<40){
				randomluku=6;
			}else if(randomluku<80){
				randomluku=12;
			}else randomluku=-1;
		}else if(player[vastustaja].alus.numero==5){
			randomluku=random(100);
			if(randomluku<50){
				randomluku=4;
			}else if(randomluku<70){
				randomluku=2;
			}else randomluku=-1;
		}else if(player[vastustaja].alus.aselaji==13){
			randomluku=random(100);
			if(randomluku<50){
				randomluku=4;
			}else randomluku=-1;
		}else if(player[vastustaja].alus.aselaji==16){
			randomluku=random(100);
			if(randomluku<50){
				randomluku=0;
			}else randomluku=-1;
		}else randomluku=-1;
		if(player[netplayer].pisteet<50){
			if(randomluku<0 || randomluku>=aluksia)randomluku=random(aluksia);
			while(alus[randomluku].pistemaara>player[netplayer].pisteet){
				randomluku=random(aluksia);
			}
		}

		if(randomluku==-1 || randomluku>=aluksia){
			int randomluku2=random(aluksia);
			while(alus[randomluku2].pistemaara>player[netplayer].pisteet){
				randomluku2=random(aluksia);
			}
			player[netplayer].uudestaan(alus[randomluku2]);
			player[netplayer].pisteet-=alus[randomluku2].pistemaara;
		}else{
			player[netplayer].uudestaan(alus[randomluku]);
			player[netplayer].pisteet-=alus[randomluku].pistemaara;
		}
		return;

	}
	bool menuready=false;
	bool valintaready=false;
	int selection=0;
	currenttime=SDL_GetTicks()-starttime;
	int frametemp=calculatefps*currenttime/1000.0;
	int joinnumero=0;
	while(!valintaready){
		while(!menuready){
			player[localplayer].pylos=false;
			player[localplayer].palas=false;
			player[localplayer].pfire=false;
			player[localplayer].pright=false;
			player[localplayer].pleft=false;
			processEvents();
			currenttime=SDL_GetTicks()-starttime;
			if(mpstatus[netplayer]==2){
				entterpressed=false;
				player[localplayer].pylos=false;
				player[localplayer].palas=false;
				player[localplayer].pfire=false;
				player[localplayer].pright=false;
				player[localplayer].pleft=false;
			}
/*			if(ai && numero && randomluku==0) while(frametemp+50>calculatefps*currenttime/1000.0){
				frametemp=calculatefps*currenttime/1000.0;
			}*/
			if(mpstatus[netplayer]==2){
				if(randomluku!=0){
					if(randomluku>0){
						randomluku--;
						player[localplayer].palas=true;
					}
					else{
						randomluku++;
						player[localplayer].pylos=true;
					}
				}
				else player[localplayer].pfire=true;
			}
			if((entterpressed || player[localplayer].pfire)){
				player[localplayer].pfire=false;
				entterpressed=false;
				menuready=true;
			}else if((player[localplayer].pylos || player[localplayer].pright)){
				if(selection>0) selection--;
				else if(selection==0)selection=aluksia;
				player[localplayer].pylos=false;
				player[localplayer].pright=false;
			}else if((player[localplayer].palas || player[localplayer].pleft)){
				if(selection<=aluksia-1) selection++;
				else if(selection==aluksia) selection=0;
				player[localplayer].palas=false;
				player[localplayer].pleft=false;
			}
			do{
				currenttime=SDL_GetTicks()-starttime;
				framecounter=calculatefps*currenttime/1000.0;
				if(oldframecounter==framecounter) 	SDL_Delay(1);
			}	while (oldframecounter==framecounter);
			if(framecounter-oldframecounter>10) oldframecounter=framecounter-1;
			skipframes=framecounter-oldframecounter;
			for (;skipframes>0;skipframes--){
				if(hostpeli){
					hostKuuntelePeli();
					int i;
					for(i=0;i<players;i++){
						if(playeruudestaan[i]==1){
							int temp3=random(aluksia);
							if(gamemode!=DOMINATION && gamemode!=CTF && gamemode!=MUSHROOM){
								while(player[i].pisteet<alus[temp3].pistemaara){
									temp3=random(aluksia);
								}
								player[i].pisteet-=alus[temp3].pistemaara;
							}
							player[i].uudestaan(alus[temp3]);
							player[i].laitaAlkuSirpaleet();
							player[i].kuolema--;
						}
					}
					for(i=0;i<players;i++){
						if(playeruudestaan[i]==3){
							playeruudestaan[i]=1;
						}
					}
				}

				if(joinpeli){
					joinPeliNetwork();
				}
					calculatedframes++;
				calculateFrame(++oldframecounter);
				if(hostpeli) netframe++;
				if(joinpeli){
					//netframe++;
					netframe2=netframe;
				}
				if(hostpeli) hostLahetaPeli();
			}
			if(mpstatus[netplayer]==2){
				drawFrame(framecounter);
				drawedframes++;
			}
			else drawNetAlusValinta(netplayer, selection);
			updateAudio();
			if(mpstatus[netplayer]==2){
				player[netplayer].pfire=false;
				player[netplayer].palas=false;
				player[netplayer].pylos=false;
			}
		}
		//random-alus
		if(selection==aluksia){
			int randomluku2=random(aluksia);
			while(alus[randomluku2].pistemaara>player[netplayer].pisteet){
				randomluku2=random(aluksia);
			}
			if(hostpeli) player[netplayer].uudestaan(alus[randomluku2]);
			if(hostpeli) player[netplayer].pisteet-=alus[randomluku2].pistemaara;
			valintaready=true;
			joinnumero=randomluku2;
		}
		//vähemmän random
		else{
			if(player[netplayer].pisteet>=alus[selection].pistemaara){
				if(hostpeli) 
					player[netplayer].uudestaan(alus[selection]);
				if(hostpeli) 
					player[netplayer].pisteet-=alus[selection].pistemaara;
				valintaready=true;
			}else {
				selection=2;
				menuready=false;
			}
			joinnumero=selection;
		}
		escpressed=false;
	}
	if(joinpeli){
//		int i;
		//char viesti[joinaavia*sizeof(napit)+10];
		//char *viesti;
		//viesti=(char*)malloc(10);
		char viesti[10];
		sprintf(viesti,"alusv");
		viesti[5]=(char)netplayer;
		viesti[6]=(char)joinnumero;
		viesti[7]=';';
		viesti[8]='\0';
	
		int len=9;
		int result=SDLNet_TCP_Send(joinsoketti, viesti,len);
		if(result<len) {
			printf("virhe lähetettäessä alustietoja\n");
		}
		//free(viesti);
	}
}
