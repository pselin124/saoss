#include "main.h"
#include "domination.h"
#include "ctf.h"
#include "net.h"

int cpisteita=MAXNETPLAYERS;
CPiste *cpiste, *alkupiste;
float ctfpisteet=25;
float ckuolemapisteet=1;

void alustaCPisteet(){
	cpiste=(CPiste*)malloc(sizeof(CPiste)*cpisteita);
	alkupiste=(CPiste*)malloc(sizeof(CPiste)*cpisteita);
	int i;
	for(i=0;i<cpisteita;i++){
		cpiste[i].player=100;
		cpiste[i].numero=i;
		if(i==0){
			cpiste[i].x=maxx-20;
			cpiste[i].y=maxy-20;
		}else if(i==1){
			cpiste[i].x=minx+20;
			cpiste[i].y=miny+20;
		}else if(i==2){
			cpiste[i].x=maxx-20;
			cpiste[i].y=miny+20;
		}else if(i==3){
			cpiste[i].x=minx+20;
			cpiste[i].y=maxy-20;
		}else{
			cpiste[i].x=0;
			cpiste[i].y=0;
		}
	}
	for(i=0;i<cpisteita;i++){
		alkupiste[i].x=cpiste[i].x;
		alkupiste[i].y=cpiste[i].y;
	}
}

void piirraCGraffat(){
	glPushMatrix();
	glEnable(GL_BLEND);
	glDisable(GL_DEPTH);
	glBlendFunc(GL_ONE,GL_ONE);	
	glDisable(GL_LIGHTING);
	
	//glTranslatef(camposx,camposy,camposz-30.0f);

	glTranslatef(-8.5f, +10.0f, 0);

	char numero2[3];
	char numero3[8];
	char numero4[8];

	bool laitettu[MAXNETPLAYERS];
	int i;
	for(i=0;i<players;i++){
		laitettu[i]=false;
	}
	int maara=0;
	glColor4f(0.2,0.2,0.8,0.8);
	gl_println( 1, 2,		0,"capture the flower score",	0.40);
	glColor4f(0.2,0.5,0.2,0.5);
	for(i=0;i<players;i++){
		if(!laitettu[player[i].tiimi]){
			maara++;
			laitettu[player[i].tiimi]=true;
			itoa(player[i].tiimi+1,numero2,10);	
			itoa(player[i].pisteet,numero3,10);	
			if(hostpeli || joinpeli) itoa(mpcredits,numero4,10);	
			else itoa(fullcredits,numero4,10);	

			gl_println( 0, 1,		0,"team",	0.25);
			gl_println( 2, 1,		0,numero2,	0.25);
			if(player[i].pisteet<1000) gl_println( 0, 0.5,		0,numero3,	0.25);
			else gl_println( -0.4, 0.5,		0,numero3,	0.25);
			gl_println( 1.3, 0.5,		0,"/",		0.25);
			gl_println(1.6, 0.5,		0,numero4,	0.25);

			glTranslatef(3.5, 0, 0);
			if(maara==5 || maara==10) glTranslatef(-3.5*5,-1.2,0);
		}
	}
	
	glPopMatrix();
}

void laskeCPisteet(){
	int i,j;
	for(i=0;i<cpisteita;i++) if(cpiste[i].enabled){
		if(cpiste[i].player<100){
			int ppelaaja=cpiste[i].player;
			int ptiimi=player[ppelaaja].tiimi;
			if(player[ppelaaja].energia>=0 && player[ppelaaja].kuolema<1){
				float xet=player[ppelaaja].x-cpiste[ptiimi].x;
				float yet=player[ppelaaja].y-cpiste[ptiimi].y;
				float etaisyys=sqrt(xet*xet+yet*yet);
				if(etaisyys<CSADE && cpiste[ptiimi].player==100){	
					cpiste[i].player=100;
					cpiste[i].x=alkupiste[i].x;
					cpiste[i].y=alkupiste[i].y;
					int i2;
					for(i2=0;i2<players;i2++){
						if(player[i2].tiimi==ptiimi) player[i2].pisteet+=ctfpisteet;
					}
				}	
			}else{
				cpiste[i].x=player[ppelaaja].x;
				cpiste[i].y=player[ppelaaja].y;
				cpiste[i].player=101;
			}
			
		}else if(cpiste[i].player==100){
			for(j=0;j<players;j++){
				float xet=cpiste[i].x-player[j].x;
				float yet=cpiste[i].y-player[j].y;
				float etaisyys=sqrt(xet*xet+yet*yet);
				if(etaisyys<CSADE && player[j].tiimi!=i){
					cpiste[i].player=j;
					j=players;
				}

			}
		}else if(cpiste[i].player==101){
			for(j=0;j<players;j++){
				float xet=cpiste[i].x-player[j].x;
				float yet=cpiste[i].y-player[j].y;
				float etaisyys=sqrt(xet*xet+yet*yet);
				if(etaisyys<CSADE && player[j].tiimi!=i){
					cpiste[i].player=j;
					j=players;
				}else if(etaisyys<CSADE){
					cpiste[i].player=100;
					cpiste[i].x=alkupiste[i].x;
					cpiste[i].y=alkupiste[i].y;
					j=players;
				}

			}
		}

	}
}

void piirraCPisteet(){
	/* piirretään ctf-kuvat */
	int i;
	for(i=0;i<cpisteita;i++) if(cpiste[i].enabled){
		glEnable(GL_DEPTH_TEST);
		glDisable(GL_LIGHTING);
		glEnable(GL_BLEND);
		glEnable(GL_TEXTURE_2D);

		valitseVari(i);
		glPushMatrix();
			glTranslatef(alkupiste[i].x,alkupiste[i].y,alustaso);
			glBindTexture(GL_TEXTURE_2D, texture[18]);
			double koko=10;
			glBegin(GL_QUADS);
				glNormal3f(0,0,1.0f);
				glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*koko, -1.0f*koko,  1.0f);	// Bottom Left Of The Texture and Quad
				glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*koko, -1.0f*koko,  1.0f);	// Bottom Right Of The Texture and Quad
				glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*koko,  1.0f*koko,  1.0f);	// Top Right Of The Texture and Quad
				glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*koko,  1.0f*koko,  1.0f);	// Top Left Of The Texture and Quad
			glEnd();
			if(cpiste[i].player==100){
				glEnable(GL_DEPTH_TEST);
				glEnable(GL_LIGHTING);
				glDisable(GL_BLEND);
				glDisable(GL_TEXTURE_2D);
				glScalef(5,5,5);
				glRotatef(90,1,0,0);
				glCallList(KUKKA4);
			}else if(cpiste[i].player==101){
				glPopMatrix();
				glPushMatrix();	

				glEnable(GL_DEPTH_TEST);
				glEnable(GL_LIGHTING);
				glDisable(GL_BLEND);
				glDisable(GL_TEXTURE_2D);
				glTranslatef(cpiste[i].x,cpiste[i].y,alustaso);
				glScalef(5,5,5);
				glRotatef(90,1,0,0);
				glCallList(KUKKA4);
			
			}
		glPopMatrix();
	}
}
