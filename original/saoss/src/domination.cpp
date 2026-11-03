#include "main.h"
#include "domination.h"
#include "ctf.h"
#include "net.h"

int dpisteita=2;
float pisteetfreimissa=0.005;
DPiste *dpiste;

void alustaDPisteet(){
	dpiste=(DPiste*)malloc(sizeof(DPiste)*dpisteita);
	int i;
	dpiste[0].x=maxx-20;
	dpiste[0].y=maxy-20;
	dpiste[1].x=minx+20;
	dpiste[1].y=miny+20;
	for(i=0;i<dpisteita;i++){
		dpiste[i].tiimi=-1;
	}
}

void piirraDGraffat(){
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
	gl_println( 1, 2,		0,"domination score",	0.40);
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

void laskeDPisteet(){
	//int *temptiimi;
	//temptiimi=(int*)malloc(sizeof(int)*dpisteita);
	int temptiimi[16];
	int i,j;
	for(i=0;i<dpisteita;i++){
		temptiimi[i]=-1;
	}
	for(i=0;i<dpisteita;i++){
		for(j=0;j<players;j++){
			float xet=dpiste[i].x-player[j].x;
			float yet=dpiste[i].y-player[j].y;
			float etaisyys=sqrt(xet*xet+yet*yet);
			if(etaisyys<DSADE){
				temptiimi[i]=player[j].tiimi;
				j=players;
			}
		}
	}
	for(i=0;i<dpisteita;i++){
		if(temptiimi[i]>=0) for(j=0;j<players;j++){
			float xet=dpiste[i].x-player[j].x;
			float yet=dpiste[i].y-player[j].y;
			float etaisyys=sqrt(xet*xet+yet*yet);
			if(etaisyys<DSADE && temptiimi[i]!=player[j].tiimi){
				temptiimi[i]=-1;
				j=players;
			}
		}
		if(temptiimi[i]>=0){
			dpiste[i].tiimi=temptiimi[i];
		}
		if(dpiste[i].tiimi>=0){
			for(j=0;j<players;j++){
				if(player[j].tiimi==dpiste[i].tiimi) player[j].pisteet+=pisteetfreimissa;
			}
		}
	}
}
void piirraDPisteet(){
	/* piirretään domination-kuvat */
	glEnable(GL_DEPTH_TEST);
	glEnable(GL_LIGHTING);
	glDisable(GL_BLEND);
	glDisable(GL_TEXTURE_2D);
	int i;
	for(i=0;i<dpisteita;i++){
		glColor4f(1,1,1,1);
		if(dpiste[i].tiimi>=0){
			valitseVari(dpiste[i].tiimi);
		}
		glPushMatrix();
			glTranslatef(dpiste[i].x,dpiste[i].y,alustaso);
			glScalef(8,8,8);
			glRotatef(90,1,0,0);
			glCallList(KUKKA4);
		glPopMatrix();
	}
}
