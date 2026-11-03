#include "main.h"
#include "sieni.h"
#include "net.h"

SPiste *spiste;
int spisteita=1;
float sienipisteet=10;
float skuolemapisteet=1;

void alustaSPisteet(){
	spiste=(SPiste*)malloc(sizeof(SPiste)*spisteita);
	int i;
	for(i=0;i<spisteita;i++){
		spiste[i].x=0;
		spiste[i].y=50;
	}
}

void laskeSPisteet(){
	int i,j,temptiimi=-1;
	for(i=0;i<spisteita;i++){
		for(j=0;j<players;j++){
			float xet=spiste[i].x-player[j].x;
			float yet=spiste[i].y-player[j].y;
			float etaisyys=sqrt(xet*xet+yet*yet);
			if(etaisyys<SSADE){
				temptiimi=player[j].tiimi;
				j=players;
			}
		}
		if(temptiimi>=0){
			for(j=0;j<players;j++){
				if(player[j].tiimi==temptiimi){
					player[j].pisteet+=sienipisteet;
				}
			}
			spiste[i].x=0;
			spiste[i].y=0;
			spiste[i].uusiks=true;
			while(spiste[i].x*spiste[i].x+spiste[i].y*spiste[i].y<500){
				spiste[i].x=random(maxx*2000.0f)/1000.0f-maxx;
				spiste[i].y=random(maxy*2000.0f)/1000.0f-maxy;
			}
		
		}
	}
}

void piirraSPisteet(){
	/* piirretään sienet */ 
	glEnable(GL_DEPTH_TEST);
	glEnable(GL_LIGHTING);
	glDisable(GL_BLEND);
	glDisable(GL_TEXTURE_2D);
	int i;
	for(i=0;i<spisteita;i++){
		glColor4f(1,1,1,1);
		glPushMatrix();
			glTranslatef(spiste[i].x,spiste[i].y,alustaso);
			glScalef(5,5,5);
			glRotatef(90,1,0,0);
			glCallList(SIENI);
		glPopMatrix();
	}

}

void piirraSGraffat(){
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
	gl_println( 1, 2,		0,"mushroom score",	0.40);
	glColor4f(0.2,0.5,0.2,0.5);
	for(i=0;i<players;i++){
		if(!laitettu[player[i].tiimi]){
			maara++;
			laitettu[player[i].tiimi]=true;
			itoa(player[i].tiimi+1,numero2,10);	
			itoa(player[i].pisteet,numero3,10);	
			if(joinpeli || hostpeli) itoa(mpcredits,numero4,10);	
			else  itoa(fullcredits,numero4,10);	

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
