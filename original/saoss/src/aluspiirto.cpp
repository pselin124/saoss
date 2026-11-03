#include "main.h"
#include "Define.h"
#include "aluspiirto.h"

void alus0(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_0_OSA_1, GL_COMPILE);
		gah=gluNewQuadric();
			glScalef(1, 2.6, 0.6);
			gluSphere(gah, 0.5, 15+sets->detail*10, 5+8*sets->detail);
		gluDeleteQuadric(gah);
	glEndList();

	glNewList(DRAW_ALUS_0_OSA_2, GL_COMPILE);
		gah=gluNewQuadric();
			glScalef(1, 1.0/2.6, 1.0/0.6);
			glTranslatef(0, 0.5, 0.1);
			glScalef(1,2.3,1);
			glEnable(GL_BLEND);
			glBlendFunc(GL_ONE, GL_ONE_MINUS_SRC_ALPHA);
			gluSphere(gah, 0.3, 15+sets->detail*10, 5+8*sets->detail);
			glDisable(GL_BLEND);
		gluDeleteQuadric(gah);
	glEndList();

	glNewList(DRAW_ALUS_0_OSA_3, GL_COMPILE);
		gah=gluNewQuadric();
			gluSphere(gah, 0.5, 15+sets->detail*10, 5+8*sets->detail);
			glTranslatef(-2.6/0.7, 0, 0);
			gluSphere(gah, 0.5, 15+sets->detail*10, 5+8*sets->detail);
			glScalef(1.0/0.7,1,1);
			glTranslatef(0, 0.15, 0);
			glRotatef(-90,1,0,0);
			glColor3f(0.6, 0.6, 0.6);
			gluCylinder(gah, 0.08,0.03,1.5, 5+sets->detail*5, 1+sets->detail*2);
			glTranslatef( 2.6, 0, 0);
			gluCylinder(gah, 0.08,0.03,1.5, 5+sets->detail*5, 1+sets->detail*2);
		gluDeleteQuadric(gah);
	glEndList();
}


void alus1(){
	GLUquadricObj *gah;

	glNewList(DRAW_ALUS_1, GL_COMPILE);
		gah=gluNewQuadric();
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glDisable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);			
			glPushMatrix();
			glTranslatef(0, -0.5, 0);


			glPushMatrix();
			glColor3f(0.7, 0.5, 0.0);
			glTranslatef(-0.7, 0.5, 0);
			glRotatef(60, 0, 0, 1);
			glScalef(1.5, 0.2, 0.7);
			glCallList(DRAW_CUBE);
			glPopMatrix();

			glPushMatrix();
			glColor3f(0.7, 0.5, 0.0);
			glTranslatef(0.7, 0.5, 0);
			glRotatef(-60, 0, 0, 1);
			glScalef(1.5, 0.2, 0.7);
			glCallList(DRAW_CUBE);
			glPopMatrix();

			glPushMatrix();
			glColor3f(1, 0.5, 1);
			glTranslatef(0, 0, 0.5);
			glScalef(1.5, 1, 1);
			gluSphere(gah,0.5,8+sets->detail*15,5+sets->detail*10);
			glPopMatrix();

			glPushMatrix();
			glColor3f(1, 1, 1);
			glTranslatef(0, 0, 0.5);
			glRotatef(-90, 1, 0, 0);
			glTranslatef(-1, 1, 0);
			gluSphere(gah,0.5,6+sets->detail*10, 5+sets->detail*7);
			gluCylinder(gah, 0.2,0.1,2, 5+sets->detail*5, 1+sets->detail*2);
			glTranslatef(1*2, 0, 0);
			gluSphere(gah,0.5,6+sets->detail*10, 5+sets->detail*7);
			gluCylinder(gah, 0.2,0.1,2, 5+sets->detail*5, 1+sets->detail*2);
			glPopMatrix();

			glPopMatrix();
			glEnable(GL_TEXTURE_2D);
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
			glDisable(GL_DEPTH_TEST);			
		gluDeleteQuadric(gah);
	glEndList();
}

void alus2(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_2, GL_COMPILE);
		gah=gluNewQuadric();
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glDisable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);			

			
			//ruumis
			glPushMatrix();
			glColor3f(1, 0.1, 0.1);
			glTranslatef(0, -1, 0);
			glRotatef(-90, 1, 0, 0);
			gluCylinder(gah, 0.5, 0.5, 2, 10+10*sets->detail, 1+2*sets->detail);
			glPopMatrix();

			//päällystäsylinterit
			//keltaset
			glPushMatrix();
			glColor3f(1, 0.9, 0.1);
			glTranslatef(0, -0.8, 0);
			glRotatef(-90, 1, 0, 0);
			gluCylinder(gah, 0.52, 0.52, 0.2, 10+10*sets->detail, 1+2*sets->detail);
			glTranslatef(0,  0, 0.8);
			gluCylinder(gah, 0.52, 0.52, 0.2, 10+10*sets->detail, 1+2*sets->detail);
			glTranslatef(0,  0, 0.8);
			gluCylinder(gah, 0.52, 0.52, 0.1, 10+10*sets->detail, 1+2*sets->detail);
			//liilat
			glColor3f(1, 0.15, 1);
			glTranslatef(0,  0, -0.2);
			gluCylinder(gah, 0.52, 0.52, 0.2, 10+10*sets->detail, 1+2*sets->detail);
			glTranslatef(0,  0, -0.8);
			gluCylinder(gah, 0.52, 0.52, 0.2, 10+10*sets->detail, 1+2*sets->detail);
			glTranslatef(0,  0, -0.7);
			gluCylinder(gah, 0.52, 0.52, 0.1, 10+10*sets->detail, 1+2*sets->detail);
			glPopMatrix();

			//siivet
			glPushMatrix();
			glColor3f(1, 0.15, 0.5);
			glScalef(2, 1, 0.05);
			gluSphere(gah, 1, 10+8*sets->detail, 5+5*sets->detail);
			glPopMatrix();

			glPushMatrix();
			glColor3f(1, 0.15, 0.9);
			glScalef(1.4, 0.5, 0.1);
			gluSphere(gah, 1, 4, 4);
			glPopMatrix();

			//nokka
			glPushMatrix();
			glColor3f(1, 0.8, 0.6);
			glTranslatef(0,  1, 0);
			glScalef(1, 0.75, 1);
			gluSphere(gah, 0.5, 10+10*sets->detail, 5+10*sets->detail);
			glPopMatrix();

			//silmät
			glPushMatrix();
			glColor3f(0.2, 0.2, 0.7);
			glTranslatef(0.2, 1.15, 0.2);
			gluSphere(gah, 0.2, 10, 5);
			glTranslatef(-0.4, 0, 0);
			gluSphere(gah, 0.2, 10+10*sets->detail, 5+5*sets->detail);
			glPopMatrix();

			//peppu
			glPushMatrix();
			glColor3f(1, 0.5, 0.2);
			glTranslatef(0,  -1, 0);
			glScalef(1, 0.3, 1);
			gluSphere(gah, 0.5, 13+10*sets->detail, 5+10*sets->detail);
			glPopMatrix();

			//häntä
			glColor3f(0.6, 0.4, 0.2);
			glPushMatrix();
			glTranslatef(0, -0.7, 0);
//			glRotatef(90,0,1,0);
//			glRotatef(sin(player[numero2].propelli/50.0f)*35, 1, 0, 0);
			glScalef(0.2, 1, 0.2);
			gluSphere(gah, 1, 6+8*sets->detail, 8+8*sets->detail);
			glPopMatrix();

			
			glEnable(GL_TEXTURE_2D);
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
			glDisable(GL_DEPTH_TEST);			
		gluDeleteQuadric(gah);
	glEndList();
}

void alus3(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_3, GL_COMPILE);
		gah=gluNewQuadric();
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glDisable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);			

			glTranslatef(0,-0.5,0);

			//keppi
			glPushMatrix();
				glColor3f(0.6, 0.50, 0.5);
//			glTranslatef(0, 0, 0);
				glRotatef(90,0,1,0);
				glTranslatef(0,0,-1.5);
				gluCylinder(gah, 0.3, 0.3, 3, 7+5*sets->detail, 1+1*sets->detail);

				glRotatef( 180, 1, 0, 0);
				//glRotatef( 90, 0, 0, 0);
				gluDisk(gah, 0,0.3,7+5*sets->detail,1+1*sets->detail);

				glRotatef(-90, 1, 0, 0);
				glTranslatef(0, 3, 0);
				glRotatef(-90, 1, 0, 0);
				gluDisk(gah, 0,0.3,7+5*sets->detail,1+1*sets->detail);
			glPopMatrix();

			//teräjuttu
			glPushMatrix();
				glColor3f(0.8, 0.1, 0.1);
				glScalef(0.5, 1.2, 0.15);
				gluSphere(gah, 1, 4, 5+8*sets->detail);
				glScalef(1.0/0.5, 1.0/1.2, 1.0/0.15);
				glTranslatef(0,-0.7,0);
				glColor3f(0.6, 0.4, 0.4);
				glRotatef(10,0,1,0);
				gluDisk(gah, 0,1.0,10+5*sets->detail,1+1*sets->detail);
				glRotatef(-20,0,1,0);
				gluDisk(gah, 0,1.0,10+5*sets->detail,1+1*sets->detail);
			glPopMatrix();

			//aseitten varret
			glPushMatrix();
				glColor3f(0.55, 0.5, 0.5);
				glTranslatef(1.2,0.5,0);
				glScalef(1, 2.5, 1);
				gluSphere(gah, 0.25, 5+8*sets->detail, 4+8*sets->detail);
				glTranslatef(-2.4,00,0);
				gluSphere(gah, 0.25, 5+8*sets->detail, 4+8*sets->detail);
			glPopMatrix();

			glPushMatrix();
				glColor3f(0.55, 0.5, 0.5);
				glTranslatef(1.2,0.75,0);
				glRotatef(-90,1,0,0);
				gluCylinder(gah, 0.15, 0.12, 1, 5+5*sets->detail, 1+1*sets->detail);
				glTranslatef(-2.4,0,0);
				gluCylinder(gah, 0.15, 0.12, 1, 5+5*sets->detail, 1+1*sets->detail);
			glPopMatrix();

			glEnable(GL_TEXTURE_2D);
			glDisable(GL_DEPTH_TEST);			
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
		gluDeleteQuadric(gah);
	glEndList();
}

void alus4(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_4_OSA_1, GL_COMPILE);
		gah=gluNewQuadric();
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glDisable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);			

			glTranslatef(0,-0.4,0);
			glRotatef(90, 1, 0, 0);


			glPushMatrix();
			glColor3f(0.8, 0.8, 1);
			gluCylinder(gah, 0.1, 0.1, 1, 5+8*sets->detail, 2+2*sets->detail);
		
			glPushMatrix();
			glTranslatef(0,0,1);
			gluDisk(gah, 0,0.1,5+8*sets->detail,2+2*sets->detail);
			glTranslatef(0,0,-1);
			glRotatef(180, 0, 1, 0);
			gluDisk(gah, 0,0.1,5+8*sets->detail,2+2*sets->detail);
			glPopMatrix();

			glColor3f(1, 1, 1);

		gluDeleteQuadric(gah);
	glEndList();


	glNewList(DRAW_ALUS_4_OSA_2, GL_COMPILE);
		gah=gluNewQuadric();
			glPopMatrix();

			glPushMatrix();
			glColor3f(0.8, 0.8, 1);
			glScalef(1.5, 0.2, 0.03);
			glTranslatef(0, 0, 3);
//			drawCube();
			glCallList(DRAW_CUBE);
			glPopMatrix();
			
			{
				//eka sylinteri
				glPushMatrix();
				glColor3f(1, 1, 0);
				glTranslatef(1, 0, 0.1);
				gluCylinder(gah, 0.1, 0.3, 0.7, 5+8*sets->detail, 2+2*sets->detail);

				//eka diski
/*				if(kaasutus){
					glColor3f(0.9, 0.1, 0.1);
					glTranslatef(0, 0, 0.7);
					gluDisk(gah, 0,0.3,5+8*sets->detail,2+2*sets->detail);
					glTranslatef(0, 0, -0.7);
				}
*/
				//toka sylinteri
				glTranslatef(-2, 0, 0);
				glColor3f(1, 1, 0);
				gluCylinder(gah, 0.1, 0.3, 0.7, 5+8*sets->detail, 2+2*sets->detail);

				//toka diski
/*				if(kaasutus){
					glColor3f(0.9, 0.1, 0.1);
					glTranslatef(0, 0, 0.7);
					gluDisk(gah, 0,0.3,5+8*sets->detail,2+2*sets->detail);
				}
*/
				glPopMatrix();
			}
			glPushMatrix();
			glScalef(1, 0.1, 1);
			glColor3f(1, 0.7, 1);
			glTranslatef(0, 0, -1.4);
			gluCylinder(gah, 0.8, 1.2, 1, 5, 2);
			glPopMatrix();

			glPushMatrix();
			glTranslatef(0, 0, -2);
			glColor3f(1, 1, 1);
			glPushMatrix();
			glTranslatef(0.6, 0, 0);
			glRotatef(20, 0, 1, 0);
			glScalef(0.7,1,1);
			gluCylinder(gah, 0.2, 0.2, 2.2, 5+8*sets->detail, 2+2*sets->detail);
			glPopMatrix();
			glTranslatef(-0.6, 0, 0);
			glRotatef(-20, 0, 1, 0);
			glScalef(0.7,1,1);
			gluCylinder(gah, 0.2, 0.2, 2.2, 5+8*sets->detail, 2+2*sets->detail);
			glPopMatrix();

			glPushMatrix();
			glColor3f(1, 0.9, 1);
			glTranslatef(0, 0, -1.9);
			glScalef(1.7, 0.6, 0.3);
			gluSphere(gah, 0.5, 10+15*sets->detail, 5+8*sets->detail);
			glPopMatrix();

			glPushMatrix();
			glColor3f(0, 0, 1);
//			glPushMatrix();
			glTranslatef(0.3, 0, -0.8);
			gluSphere(gah, 0.2, 10+10*sets->detail, 5+8*sets->detail);
//			glPopMatrix();
//			glPushMatrix();
//			glTranslatef(-0.3, 0, -0.8);
			glTranslatef(-0.6, 0, 0);
			gluSphere(gah, 0.2, 10+10*sets->detail, 5+8*sets->detail);
//			glPopMatrix();
//			glPushMatrix();
//			glTranslatef(0, 0, -1.0);
			glTranslatef(0.3, 0, -0.2);
			gluSphere(gah, 0.2, 10+10*sets->detail, 5+8*sets->detail);
//			glPopMatrix();
			glPopMatrix();

			glDisable(GL_DEPTH_TEST);			
			glEnable(GL_TEXTURE_2D);
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
		gluDeleteQuadric(gah);
	glEndList();

}

void alus5(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_5, GL_COMPILE);
		gah=gluNewQuadric();
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glDisable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);			

//			glTranslatef(0,-1.2,0);


			//runko
			glPushMatrix();
				glColor3f(0.65, 0.6, 0.4);
				glTranslatef(0, 1, 0);
				glRotatef(90,1,0,0);
				glScalef(1, 0.5, 1);
				gluCylinder(gah, 0.5, 0.5, 2, 15+5*sets->detail, 3+2*sets->detail);
				gluSphere(gah, 0.5, 15+sets->detail*5, 8+8*sets->detail);
				glTranslatef(0,0,2);
				glScalef(1, 1, 0.25);
				gluSphere(gah, 0.5, 15+sets->detail*5, 8+8*sets->detail);
			glPopMatrix();


			//vauhtitorvi
			glPushMatrix();
				glColor3f(0.65, 0.6, 0.6);
				glTranslatef(0, -1, 0);
				glRotatef(90, 1, 0, 0);
				glScalef(1, 0.75, 1);
				gluCylinder(gah, 0.25, 0.27, 0.45, 7+5*sets->detail, 1+2*sets->detail);
			glPopMatrix();
			
			//vauhtiviivat
			glPushMatrix();
				glColor3f(0.1, 0.05, 0.8);
				glTranslatef(0, 0.4, 0.2);
				glScalef(0.2, 1, 0.5);
				gluSphere(gah, 0.3, 6+sets->detail*8, 5+6*sets->detail);
				glTranslatef(0, -0.8, 0);
				gluSphere(gah, 0.3, 6+sets->detail*8, 5+6*sets->detail);
			glPopMatrix();

			//etutorvi
			glPushMatrix();
				glColor3f(0.65, 0.6, 0.4);
				glTranslatef(0, 0, -0.3);
				glScalef(0.5, 0.3, 1);
				gluSphere(gah, 0.55, 6+sets->detail*8, 5+6*sets->detail);
			glPopMatrix();
			glPushMatrix();
				glColor3f(0.65, 0.6, 0.6);
				glTranslatef(0, 0, -0.6);
				glRotatef(-90, 1, 0, 0);
				glScalef(1, 0.75, 1);
				gluCylinder(gah, 0.2, 0.5, 1.2, 7+5*sets->detail, 1+2*sets->detail);
			glPopMatrix();

			//takapiippu1
			glPushMatrix();
				glColor3f(0.65, 0.6, 0.4);
				glTranslatef(0.3, 0, -0.1);
				glRotatef(110, 0, 1, 0);
				glRotatef( 20, 1, 0, 0);
				glScalef(0.3, 0.6, 1);
				gluCylinder(gah, 0.2, 0.5, 0.7, 7+5*sets->detail, 1+2*sets->detail);
			glPopMatrix();
			glPushMatrix();
				glColor3f(0.60, 0.6, 0.6);
				glTranslatef(0.9, -1.05, -0.3);
				glRotatef(-90, 1, 0, 0);
				gluDisk(gah, 0,0.15,7+4*sets->detail,1+2*sets->detail);
				gluCylinder(gah, 0.15, 0.15, 1.25, 7+5*sets->detail, 1+2*sets->detail);
				glTranslatef(0,0,1.25);
				gluDisk(gah, 0,0.15,7+4*sets->detail,1+2*sets->detail);
			glPopMatrix();

			//takapiippu2
			glPushMatrix();
				glColor3f(0.65, 0.6, 0.4);
				glTranslatef(-0.3, 0, -0.1);
				glRotatef(-110, 0, 1, 0);
				glRotatef( 20, 1, 0, 0);
				glScalef(0.3, 0.6, 1);
				gluCylinder(gah, 0.2, 0.5, 0.7, 7+5*sets->detail, 1+2*sets->detail);
			glPopMatrix();
			glPushMatrix();
				glColor3f(0.60, 0.6, 0.6);
				glTranslatef(-0.9, -1.05, -0.3);
				glRotatef(-90, 1, 0, 0);
				gluDisk(gah, 0,0.15,7+4*sets->detail,1+2*sets->detail);
				gluCylinder(gah, 0.15, 0.15, 1.25, 7+5*sets->detail, 1+2*sets->detail);
				glTranslatef(0,0,1.25);
				gluDisk(gah, 0,0.15,7+4*sets->detail,1+2*sets->detail);
			glPopMatrix();

			glEnable(GL_TEXTURE_2D);
			glDisable(GL_DEPTH_TEST);			
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
		gluDeleteQuadric(gah);
	glEndList();

}

void alus6(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_6_OSA_1, GL_COMPILE);
		gah=gluNewQuadric();
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glDisable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);

			glPushMatrix();
			glColor3f(0, 0, 1);
			glScalef(1.5, 1.5, 0.25);
			gluSphere(gah, 1.0, 14+12*sets->detail, 12+12*sets->detail);
			glPopMatrix();
		
			glPushMatrix();
			glColor3f(0, 0.2, 1);
			gluSphere(gah, 1.0, 12+10*sets->detail, 8+8*sets->detail);
			glPopMatrix();
		
			glPushMatrix();
			glColor3f(1, 1, 1);
			glTranslatef(0, 0, 0.5);
			glRotatef(-90, 1, 0, 0);

		gluDeleteQuadric(gah);
	glEndList();

	glNewList(DRAW_ALUS_6_OSA_2, GL_COMPILE);
		gah=gluNewQuadric();
			glTranslatef(0, 0, 0);
			gluCylinder(gah, 0.2, 0.1, 2, 5+5*sets->detail, 2+2*sets->detail);
			glPopMatrix();

			glEnable(GL_TEXTURE_2D);
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
			glDisable(GL_DEPTH_TEST);			
		gluDeleteQuadric(gah);
	glEndList();

}

void alus7(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_7, GL_COMPILE);
		gah=gluNewQuadric();
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glDisable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);			


			//keskussylinteri
			glPushMatrix();
			glColor3f(0.6, 0.6, 0.6);
			glTranslatef(0, -1, 0);
			glRotatef(-90, 1, 0, 0);
			gluCylinder(gah, 0.7, 0.7, 2, 10+5*sets->detail, 1+2*sets->detail);
			glPopMatrix();

			//keskussylinterin tukkeet,
			glPushMatrix();
			glColor3f(0.6, 0.6, 0.6);
			glTranslatef(0, -1, 0);
			glScalef(1, 0.15, 1);
			gluSphere(gah, 0.7, 10+5*sets->detail, 10+5*sets->detail);
			glPopMatrix();

			glPushMatrix();
			glColor3f(0.6, 0.6, 0.6);
			glTranslatef(0, 1, 0);
			glScalef(1, 0.15, 1);
			gluSphere(gah, 0.7, 10+5*sets->detail, 10+5*sets->detail);
			glPopMatrix();

			//vauhtipallo
			glPushMatrix();
			glColor3f(0.7, 0.5, 0.5);
			glTranslatef(0, 0, 0.6);
			glScalef(0.35, 2, 1);
			gluSphere(gah, 0.3, 10+5*sets->detail, 4+4*sets->detail);
			glPopMatrix();

			//isompi kärkikolmio
			glPushMatrix();
			glColor3f(0.6, 0.6, 0.6);
			glTranslatef(0, 1.0, 0);
			glScalef(0.7, 1.5, 0.3);
			gluSphere(gah, 1, 4, 4+8*sets->detail);
			glPopMatrix();

			//pienempi kärkikolmio
			glPushMatrix();
			glColor3f(0.8, 0.3, 0.3);
			glTranslatef(0, 1.0, 0);
			glScalef(0.525, 1.125, 0.3375);
			gluSphere(gah, 1, 4, 4+8*sets->detail);
			glPopMatrix();

			//siivet
			glPushMatrix();
			glColor3f(0.6, 0.6, 0.6);
			glTranslatef(0.7, 1, 0);
			glRotatef(180+45,0.0f,0.0f,1.0f);	
			gluPartialDisk(gah, 0, 2, 10, 1, 0, 45);
			glPopMatrix();

			glPushMatrix();
			glColor3f(0.6, 0.6, 0.6);
			glTranslatef(-0.7, 1, 0);
			glRotatef(180,0.0f,0.0f,1.0f);	
			gluPartialDisk(gah, 0, 2, 10, 1, 0, 45);
			glPopMatrix();

			//moottori
			glPushMatrix();
			glColor3f(0.5, 0.5, 0.5);
			glTranslatef(0, -1, 0);
			glRotatef(90, 1, 0, 0);
			gluCylinder(gah, 0.25, 0.35, 0.5, 8+8*sets->detail,1+2*sets->detail);
			glPopMatrix();
	
			glColor3f(0.8, 0.1, 0.1);

		gluDeleteQuadric(gah);
	glEndList();

}

void alus8(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_8, GL_COMPILE);
		gah=gluNewQuadric();
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glDisable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);			

			glTranslatef(0,0.6,0);

			//ruumis
			glPushMatrix();
			glColor3f(0.8, 0.73, 0.7);
//			glTranslatef(0, 0, 0);
			glScalef(1, 1.25, 0.5);
			gluSphere(gah, 1.0, 12+10*sets->detail, 6+5*sets->detail);
			glPopMatrix();

			//kieli
			glPushMatrix();
			glColor3f(1, 0.17, 0.63);
			glTranslatef(0, 1.25, 0);
			glScalef(1, 2.3, 0.5);
			gluSphere(gah, 0.3, 10+8*sets->detail, 5+8*sets->detail);
			glPopMatrix();

			//eka silmä
			glPushMatrix();
			glColor3f(0.1, 0.1, 0.8);
			glTranslatef(0.55, 0.7, 0.25);
			gluSphere(gah, 0.2, 10+10*sets->detail, 5+5*sets->detail);
			glColor3f(0.05, 0.05, 0.3);
			glTranslatef(0.05, 0.1, 0.10);
			gluSphere(gah, 0.08, 8+5*sets->detail, 5+3*sets->detail);
			glPopMatrix();

			//toka silmä
			glPushMatrix();
			glColor3f(0.1, 0.1, 0.8);
			glTranslatef(-0.55, 0.7, 0.25);
			gluSphere(gah, 0.2, 10+10*sets->detail, 5+5*sets->detail);
			glColor3f(0.05, 0.05, 0.3);
			glTranslatef(-0.05, 0.1, 0.10);
			gluSphere(gah, 0.08, 8+5*sets->detail, 5+3*sets->detail);
			glPopMatrix();

			//pisteitä
			glPushMatrix();
			glColor3f(0.1, 0.7, 0.1);
			glTranslatef( 0.6, 0, 0.35);
			gluSphere(gah, 0.1, 8+5*sets->detail, 5+5*sets->detail);
			glTranslatef(-0.1, -0.4, 0);
			gluSphere(gah, 0.1, 8+5*sets->detail, 5+5*sets->detail);
			glTranslatef(-0.25, -0.28, 0);
			gluSphere(gah, 0.1, 8+5*sets->detail, 5+5*sets->detail);
			glPopMatrix();
			
			//pisteitä toiselle puolelle
			glPushMatrix();
			glColor3f(0.1, 0.7, 0.1);
			glTranslatef( -0.6, 0, 0.35);
			gluSphere(gah, 0.1, 8+5*sets->detail, 5+5*sets->detail);
			glTranslatef( 0.1, -0.4, 0);
			gluSphere(gah, 0.1, 8+5*sets->detail, 5+5*sets->detail);
			glTranslatef( 0.25, -0.28, 0);
			gluSphere(gah, 0.1, 8+5*sets->detail, 5+5*sets->detail);
			glPopMatrix();

			//peppu
			glPushMatrix();
			glColor3f(0.8, 0.73, 0.7);
			glTranslatef(0, -0.9, 0);
			glRotatef( 90, 1, 0, 0);
			glScalef(1.5, 0.6, 1);
			gluCylinder(gah, 0.35, 0.5, 1.7, 8+7*sets->detail, 2+2*sets->detail);
			glTranslatef(0, 0, 1.7);
		gluDeleteQuadric(gah);
	glEndList();}

void alus9(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_9, GL_COMPILE);
		gah=gluNewQuadric();
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glDisable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);			

			glTranslatef(0,-1.2,0);
		
			glPushMatrix();
			glColor3f(0.3, 0.7, 0.1);
			glTranslatef(-0.9, 0, 0);
			glRotatef(90, 0, 0, 1);
			glScalef(0.4, 0.4, 0.4);
			glCallList(DRAW_CUBE);
			glPopMatrix();

			glPushMatrix();
//			glColor3f(0.7, 0.5, 0.0);
			glTranslatef( 0.9, 0, 0);
			glRotatef(90, 0, 0, 1);
			glScalef(0.4, 0.4, 0.4);
			glCallList(DRAW_CUBE);
			glPopMatrix();

			glPushMatrix();
//			glColor3f(0.7, 0.5, 0.0);
			glTranslatef( 0, 1.4, 0);
			glScalef(1.3, 1, 0.4);
			glCallList(DRAW_TRI);
			glPopMatrix();


			glPushMatrix();
			glColor3f(0.4, 0.7, 0.2);
			glTranslatef( 0, 1.3, 0);
//			glColor3f(0.7, 0.5, 0.0);
			glScalef(0.85, 0.85, 0.85);
			gluSphere(gah, 1.0, 15+10*sets->detail, 10+10*sets->detail);
			glPopMatrix();

			glPushMatrix();
//			glColor3f(0, 0.7, 0);
			glColor3f(1, 1, 1);
			glTranslatef(0, 1.4, 0.2);
			glRotatef(-90, 1, 0, 0);
			gluCylinder(gah, 0.3, 0.3, 2, 5+sets->detail*5, 1+sets->detail*2);
			glPopMatrix();

			glEnable(GL_TEXTURE_2D);
			glDisable(GL_DEPTH_TEST);			
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
		gluDeleteQuadric(gah);
	glEndList();



}

void alus10(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_10, GL_COMPILE);
		gah=gluNewQuadric();
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glDisable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);			

			//runko
			glPushMatrix();
				glColor3f(1, 0.15, 0);
				glTranslatef(0, 0, 0);
				glScalef(1, 2, 1);
				gluSphere(gah, 0.5, 15+sets->detail*10, 5+8*sets->detail);
			glPopMatrix();

			glPushMatrix();
//			glColor3f(1, 1, 1);
				glTranslatef(0, -1, 0);
				glRotatef(-90, 1, 0, 0);
				gluCylinder(gah, 0.5, 0.5, 1, 5+5*sets->detail, 1+2*sets->detail);
				glRotatef(-180, 1, 0, 0);
				gluDisk(gah,0,0.5,5+5*sets->detail,1+2*sets->detail);
			glPopMatrix();
			
			//nuppi
			glPushMatrix();
				glColor3f(1, 0.05, 0);
				glTranslatef(0, 1, 0);
				glScalef(0.4, 0.4, 0.4);
				gluSphere(gah, 0.5, 8+sets->detail*10, 5+5*sets->detail);
			glPopMatrix();

			//alaosa
			int i;
			glPushMatrix();
				glTranslatef(0,-1,0);
				for(i=0;i<3;i++){
					glPushMatrix();
						glRotatef(i*120,0,1,0);
						glTranslatef(0.5,0,0);
						glRotatef(30,0,0,1);
						glScalef(0.33,1,0.33);
						gluSphere(gah,0.5,7+sets->detail*6,6+sets->detail*5);
						glScalef(3,1,3);
						glTranslatef(0,-0.5,0);
						glRotatef(-30,0,0,1);
						gluSphere(gah,0.2,6+sets->detail*6,5+sets->detail*4);
					glPopMatrix();
				}
			glPopMatrix();

			glEnable(GL_TEXTURE_2D);
			glDisable(GL_DEPTH_TEST);			
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);		
		gluDeleteQuadric(gah);
	glEndList();
}

void alus11(){
	GLUquadricObj *gah;

	glNewList(DRAW_ALUS_11, GL_COMPILE);
		gah=gluNewQuadric();
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glDisable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);			



				//kolmio
			glPushMatrix();
			glColor3f(0.6, 0.6, 0.6);
			glScalef(2,1.33,0.4);
			gluSphere(gah, 1, 3, 8+8*sets->detail);
			glPopMatrix();

			//sylinteri piippuun päin
			glPushMatrix();
			glColor3f(0.6, 0.6, 0.7);
//			glTranslatef(0,0,0);
//			glScalef(1,1,1);
			glRotatef(-90.0f,1.0f,0.0f,0.0f);	
			gluCylinder(gah, 0.5, 0.4, 1, 8+8*sets->detail, 1+2*sets->detail);
			glPopMatrix();

			//sylinterin takatuke
			glPushMatrix();
			glColor3f(0.6, 0.6, 0.7);
			glScalef(1,0.75,1);
			gluSphere(gah, 0.5, 8+8*sets->detail, 5+5*sets->detail);
			glPopMatrix();

			//etupää
			glPushMatrix();
			glColor3f(0.6, 0.6, 0.8);
			glTranslatef(0,1,0);
			glScalef(0.5,1,0.5);
			gluSphere(gah, 0.8, 8+7*sets->detail, 5+5*sets->detail);
			glPopMatrix();

			//tykit, oikea
			glPushMatrix();
			glColor3f(0.6, 0.6, 0.7);
			glTranslatef(1,0,0);
			gluSphere(gah, 0.3, 7+6*sets->detail, 5+5*sets->detail);
			glRotatef(-90.0f,1.0f,0.0f,0.0f);	
			glColor3f(0.7, 0.7, 0.7);
			gluCylinder(gah, 0.15, 0.1, 0.6, 5+5*sets->detail, 1+2*sets->detail);
			glPopMatrix();

			//tykit, oikea
			glPushMatrix();
			glColor3f(0.6, 0.6, 0.7);
			glTranslatef(-1,0,0);
			gluSphere(gah, 0.3, 7+6*sets->detail, 5+5*sets->detail);
			glRotatef(-90.0f,1.0f,0.0f,0.0f);	
			glColor3f(0.7, 0.7, 0.7);
			gluCylinder(gah, 0.15, 0.1, 0.6, 5+5*sets->detail, 1+2*sets->detail);
			glPopMatrix();

			glEnable(GL_TEXTURE_2D);
			glDisable(GL_DEPTH_TEST);			
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
		gluDeleteQuadric(gah);
	glEndList();
	
}



void alus12(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_12_OSA_1, GL_COMPILE);
		gah=gluNewQuadric();

		//alkujutut
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glDisable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);			

			//keskuspötkö
			glPushMatrix();
			glColor3f(0.6f,0.6f,0.6f);			
			glTranslatef(0,-1,0);
			glRotatef(-90, 1, 0, 0);
			gluCylinder(gah, 0.2,0.2 , 2, 8+6*sets->detail, 1+2*sets->detail);
		gluDeleteQuadric(gah);
	glEndList();

	glNewList(DRAW_ALUS_12_OSA_2, GL_COMPILE);
		gah=gluNewQuadric();
			glPopMatrix();

			//alasiivet
			glPushMatrix();
			glColor3f(0.1f,0.2f,0.8f);			
			glTranslatef(-0.5,-0.7,0);
			glRotatef( 30, 0, 0, 1);
			glScalef(0.6,0.3,0.1);
			gluSphere(gah, 1, 12+6*sets->detail, 8+4*sets->detail);
			glPopMatrix();

			glPushMatrix();
			glColor3f(0.1f,0.2f,0.8f);			
			glTranslatef( 0.5,-0.7,0);
			glRotatef( -30, 0, 0, 1);
			glScalef(0.6,0.3,0.1);
			gluSphere(gah, 1, 12+6*sets->detail, 8+4*sets->detail);
			glPopMatrix();


			//keskisiivet
			glPushMatrix();
			glColor3f(0.1f,0.2f,0.8f);			
			glTranslatef(-0.7, -0.1,0);
			glRotatef( 5, 0, 0, 1);
			glScalef(0.85,0.3,0.1);
			gluSphere(gah, 1, 7+7*sets->detail, 5+5*sets->detail);
			glPopMatrix();
		
			glPushMatrix();
			glColor3f(0.1f,0.2f,0.8f);			
			glTranslatef( 0.7, -0.1,0);
			glRotatef( -5, 0, 0, 1);
			glScalef(0.85,0.3,0.1);
			gluSphere(gah, 1, 7+7*sets->detail, 5+5*sets->detail);
			glPopMatrix();

			//yläsiivet
			glPushMatrix();
			glColor3f(0.1f,0.2f,0.8f);			
			glTranslatef(-0.7,  0.75, 0);
			glRotatef(-40, 0, 0, 1);
			glScalef(0.85,0.3,0.1);
			gluSphere(gah, 1, 7+7*sets->detail, 5+5*sets->detail);
			glPopMatrix();

			glPushMatrix();
			glColor3f(0.1f,0.2f,0.8f);			
			glTranslatef( 0.7,  0.75, 0);
			glRotatef( 40, 0, 0, 1);
			glScalef(0.85,0.3,0.1);
			gluSphere(gah, 1, 7+7*sets->detail, 5+5*sets->detail);
			glPopMatrix();

			//nenä
			glPushMatrix();
			glColor3f(0.5f,0.7f,0.5f);			
			glTranslatef( 0,  1, 0);
			glScalef(0.2,0.15,0.2);
			gluSphere(gah, 1, 10+5*sets->detail, 5+4*sets->detail);
			glPopMatrix();

			glEnable(GL_TEXTURE_2D);
			glDisable(GL_DEPTH_TEST);			
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
		gluDeleteQuadric(gah);
	glEndList();

}


void alus13(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_13, GL_COMPILE);
		gah=gluNewQuadric();
			//tykkipallo
			glPushMatrix();
			glColor4f(0.35f,0.65f,0.35f,0.87f);			
			glTranslatef(0,1,0);
			gluSphere(gah, 0.35, 10+5*sets->detail, 5+4*sets->detail);
			glPopMatrix();

			//tykki
			glPushMatrix();
			glColor4f(0.45f,0.55f,0.45f,0.97f);			
			glTranslatef(0,1.15,0);
			glRotatef(-90, 1, 0, 0);
			gluCylinder(gah, 0.2, 0.45, 0.8, 12+8*sets->detail, 1+2*sets->detail);
			glPopMatrix();

			//verho
			glPushMatrix();
			glColor4f(0.3f,0.7f,0.3f,0.6f);			
			glTranslatef(0,-1.1,0);
			glScalef(1,1,0.5);
			glRotatef(-90, 1, 0, 0);
			gluCylinder(gah, 1.4,0.8 , 2.1, 12+8*sets->detail, 1);
			glPopMatrix();

			//tykkipallo2
			glPushMatrix();
			glColor4f(0.4f,0.55f,0.4f,0.9f);			
			glTranslatef(0,1,0);
			glScalef(0.8,0.2,0.5*0.8);
			gluSphere(gah, 1, 10+10*sets->detail, 8+6*sets->detail);
			glPopMatrix();

			//takaosapallo
			glPushMatrix();
			glColor4f(0.4f,0.65f,0.4f,0.7f);			
			glTranslatef(0,-1.1,0);
			glScalef(1.4,0.35,0.5*1.4);
			gluSphere(gah, 1, 10+10*sets->detail, 8+4*sets->detail);
			glPopMatrix();

			//takaosasylinteri
			glPushMatrix();
			glColor4f(0.5f,0.6f,0.5f,0.6f);			
			glTranslatef(0,-1.1,0);
			glScalef(1,1,0.5);
			glRotatef(90, 1, 0, 0);
			gluCylinder(gah, 0.5,0.7 , 0.7, 10+10*sets->detail, 1);
			glPopMatrix();

		gluDeleteQuadric(gah);
	glEndList();

}


void alus14(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_14_OSA_1, GL_COMPILE);
		gah=gluNewQuadric();
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glDisable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);			


			//pääputki
			glPushMatrix();
			glColor3f(0.7f,0.6f,0.5f);			
			glTranslatef(0, -1,0);
			glRotatef(-90.0f,1.0f,0.0f,0.0f);	
			gluCylinder(gah, 0.5, 0.3, 0.5, 15+10*sets->detail, 1+2*sets->detail);
			glTranslatef(0, 0, 0.5);
			glColor3f(0.7f,0.63f,0.56f);			
			gluCylinder(gah, 0.3, 0.15, 1.5, 15+10*sets->detail, 1+2*sets->detail);
			glPopMatrix();

		gluDeleteQuadric(gah);
	glEndList();
	glNewList(DRAW_ALUS_14_OSA_2, GL_COMPILE);
		gah=gluNewQuadric();

			//takapallo
			glPushMatrix();
			glTranslatef(0, -1,0);
			glScalef(0.5,0.3,0.5);
			gluSphere(gah, 1, 15+10*sets->detail, 8+5*sets->detail);
			glPopMatrix();


			//piikit
			glPushMatrix();
			glColor3f(0.8f,0.8f,0.8f);			
			glTranslatef(0, -0.5,0);
			glScalef(0.4,0.2,0.4);
			gluSphere(gah, 1, 12+8*sets->detail, 7+6*sets->detail);
			glScalef(1/0.4,1/0.2,1/0.4);
			glTranslatef(0, -0.1,0);
			glRotatef(-25.0f,0,1,0);	
			int i;
			for(i=0;i<9;i++){
				glRotatef(-20,1,0,0);
				glTranslatef(0, 0, -0.2);
				gluCylinder(gah, 0.1, 0.001, 1.25, 6+6*sets->detail, 1+2*sets->detail);
				glTranslatef(0, 0, 0.2);
				glRotatef( 20,1,0,0);
				glRotatef(-40.0f,0,1,0);	
			}
			glPopMatrix();

			glEnable(GL_TEXTURE_2D);
			glDisable(GL_DEPTH_TEST);			
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);		gluDeleteQuadric(gah);
	glEndList();
}


void alus15(){
	GLUquadricObj *gah;
	GLUquadricObj *gah3;
	glNewList(DRAW_ALUS_15_OSA_1, GL_COMPILE);
		gah=gluNewQuadric();
				glEnable(GL_LIGHTING);
				glDisable(GL_TEXTURE_2D);
				glEnable(GL_BLEND);
				glEnable(GL_DEPTH_TEST);			

				glBlendFunc(GL_SRC_ALPHA,GL_ONE_MINUS_SRC_ALPHA);

				//takasylinteri
				glPushMatrix();
				glTranslatef(0, -1, 0);
				glColor4f(0.5f,0.5f,0.5f,1.0f);			
				glRotatef(90, 1, 0, 0);
				gluCylinder(gah, 0.5, 0.5, 0.5, 20+15*sets->detail, 1+2*sets->detail);
				glPopMatrix();

				glPushMatrix();
				glColor4f(0.8f,0.1f,0.1f,0.8f);			
				glTranslatef(0, -1.35, 0);
		gluDeleteQuadric(gah);
	glEndList();
	glNewList(DRAW_ALUS_15_OSA_2, GL_COMPILE);
		gah=gluNewQuadric();
				gluSphere(gah, 0.48, 20+15*sets->detail, 10+5*sets->detail);
				glPopMatrix();

				//sylinteritpallot alhaalta lähtien
				glPushMatrix();
				glColor4f(0.5f,0.5f,0.5f,1.0f);			
				glTranslatef(0, -1.0, 0);
				glScalef(1,0.2,1);
				gluSphere(gah, 0.8, 20+15*sets->detail, 7+8*sets->detail);
				glPopMatrix();

				glPushMatrix();
				glColor4f(0.6f,0.6f,0.6f,1.0f);			
				glTranslatef(0, -0.6, 0);
				glScalef(1,0.2,1);
				gluSphere(gah, 0.8, 20+15*sets->detail, 7+8*sets->detail);
				glPopMatrix();
		
				glPushMatrix();
				glColor4f(0.6f,0.6f,0.6f,1.0f);			
				glTranslatef(0, -0.2, 0);
				glScalef(1,0.2,1);
				gluSphere(gah, 0.8, 20+15*sets->detail, 7+8*sets->detail);
				glPopMatrix();
				
				glPushMatrix();
				glColor4f(0.6f,0.6f,0.6f,1.0f);			
				glTranslatef(0, 0.2, 0);
				glScalef(1,0.2,1);
				gluSphere(gah, 0.8, 20+15*sets->detail, 7+8*sets->detail);
				glPopMatrix();

				glPushMatrix();
				glColor4f(0.6f,0.6f,0.6f,1.0f);			
				glTranslatef(0, 0.6, 0);
				glScalef(1,0.2,1);
				gluSphere(gah, 1.05, 20+15*sets->detail, 7+8*sets->detail);
				glPopMatrix();

				//piippu
				glPushMatrix();
				glTranslatef(0.85, 0.6, 0);
				glColor4f(0.5f,0.5f,0.5f,1.0f);			
				glRotatef(-90, 1, 0, 0);
				gluCylinder(gah, 0.15, 0.15, 0.8, 15+10*sets->detail, 1+2*sets->detail);
				glPopMatrix();
				
				//piippupallot
				glPushMatrix();
				glColor4f(0.8f,0.1f,0.1f,0.7f);			
				glTranslatef(0.85, 1.0, 0);
				glScalef(1,0.6,1);
				gluSphere(gah, 0.2, 12+8*sets->detail, 8+6*sets->detail);
				glTranslatef(0, 0.4, 0);
				gluSphere(gah, 0.2, 12+8*sets->detail, 8+6*sets->detail);
				glPopMatrix();

				//punaset putkilot		
				glBlendFunc(GL_SRC_ALPHA,GL_ONE_MINUS_SRC_ALPHA);
				glPushMatrix();
				glColor4f(0.9f,0.1f,0.1f,0.6f);			
				glTranslatef(0, -1, 0);
				glRotatef(-90, 1, 0, 0);
				gluCylinder(gah, 0.7, 0.7, 1.5,7+8*sets->detail, 1+2*sets->detail);
				glPopMatrix();

				glPushMatrix();
				glColor4f(0.9f,0.1f,0.1f,0.6f);			
				glTranslatef(0, 0.75, 0);
				glScalef(0.9,0.5,0.9);
				gluSphere(gah, 1, 20+15*sets->detail, 8+6*sets->detail);
				glPopMatrix();

//				glPopMatrix();
				glEnable(GL_TEXTURE_2D);
				glDisable(GL_DEPTH_TEST);			
				glEnable(GL_BLEND);
				glDisable(GL_LIGHTING);
		gluDeleteQuadric(gah);
	glEndList();
	glNewList(DRAW_ALUS_15_OSA_3, GL_COMPILE);
		gah=gluNewQuadric();
		gah3=gluNewQuadric();
				glEnable(GL_LIGHTING);
				glEnable(GL_TEXTURE_2D);
				glEnable(GL_BLEND);
				glEnable(GL_DEPTH_TEST);			

//				glPushMatrix();
//				glScalef(sade/1.1f*0.65f,sade/1.1f*0.65f,sade/1.1f*0.65f);
//				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	
	//			glTranslatef(0,-1.2,0);
				glBlendFunc(GL_SRC_ALPHA,GL_ONE_MINUS_SRC_ALPHA);
	

				gluQuadricTexture(gah3,true);
				gluQuadricDrawStyle(gah3, GLU_FILL); 
				gluQuadricNormals(gah3,GLU_SMOOTH); 

				//tötterön pää
				glPushMatrix();
				glColor4f(1.0, 1.0, 1.0,1.0);
				glTranslatef(0,0.75,0);
				gluSphere(gah3, 0.5, 18+8*sets->detail, 12+6*sets->detail);
				glPopMatrix();

				//tötterön varsi
				glPushMatrix();
				glTranslatef(0,-1,0);
				glRotatef(-90, 1, 0, 0);
				gluCylinder(gah3, 0, 0.47, 1.5, 18+8*sets->detail, 1+2*sets->detail);
				glPopMatrix();

				//piikki
				glDisable(GL_TEXTURE_2D);
				glColor4f(0.75, 0.7, 0.7,1.0);
				glPushMatrix();
				glTranslatef(0,1.1,0);
				glRotatef(-90, 1, 0, 0);
				gluCylinder(gah3, 0.25, 0, 0.6, 8+8*sets->detail, 1+2*sets->detail);
				glPopMatrix();

				glEnable(GL_TEXTURE_2D);
				glDisable(GL_DEPTH_TEST);			
				glEnable(GL_BLEND);
				glDisable(GL_LIGHTING);

		gluDeleteQuadric(gah);
		gluDeleteQuadric(gah3);
	glEndList();
}

void alus16(){

	double silmakoko, putkil, silmax, putkip;

	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_16_OSA_1, GL_COMPILE);
		gah=gluNewQuadric();
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glDisable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);	
			//kolmio
			glColor3f(0.1, 0.8, 0.1);
			glPushMatrix();
			glTranslatef( 0, -0.25, 0);
			glRotatef(180, 0, 1, 0);
			glRotatef(180, 1, 0, 0);
			glScalef(1.5, 1, 0.5);
			//drawTri();

			gluSphere(gah,1,3,25+sets->detail*15);
			glPopMatrix();
			putkil=0.15;
			silmax=0.7;
			putkip=1;

			glPushMatrix();
			glColor3f(0.8, 0.1, 0.1);
			glTranslatef(silmax, 0, 0);
			glRotatef(-90, 1, 0, 0);
			gluCylinder(gah, putkil, putkil, putkip, 10+10*sets->detail, 1+2*sets->detail);
			glPopMatrix();


			//silmaputket
			glColor3f(0.8, 0.1, 0.1);
			glPushMatrix();
			glTranslatef(silmax, 0, 0);
			glRotatef(-90, 1, 0, 0);
			gluCylinder(gah, putkil, putkil, putkip, 10+10*sets->detail, 1+2*sets->detail);
			glPopMatrix();

			glPushMatrix();
			glTranslatef(-1*silmax, 0, 0);
			glRotatef(-90, 1, 0, 0);
			gluCylinder(gah, putkil, putkil, putkip, 10+10*sets->detail, 1+2*sets->detail);
			glPopMatrix();

			//silmat
			silmakoko=0.5;

			glColor3f(0.1, 0.1, 0.8);
			glPushMatrix();
			glTranslatef(silmax, putkil+1, 0);
			gluSphere(gah, silmakoko, 20+18*sets->detail, 16+15*sets->detail);
			glPopMatrix();

			glPushMatrix();
			glTranslatef(-1*silmax, putkil+1, 0);
			gluSphere(gah, silmakoko, 20+18*sets->detail, 16+15*sets->detail);
			glPopMatrix();
			
			//kieli
			glColor3f(0.6, 0.1, 0.1);
			glPushMatrix();
			glTranslatef(0, -1, 0);
			glRotatef(90,0,1,0);
			
			gluDeleteQuadric(gah);
	glEndList();

	glNewList(DRAW_ALUS_16_OSA_2, GL_COMPILE);
		gah=gluNewQuadric();
			glScalef(1, 3, 0.4);
			gluSphere(gah, 0.3, 8+8*sets->detail, 5+5*sets->detail);
			glPopMatrix();
		gluDeleteQuadric(gah);
	glEndList();
}

void alus17(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_17, GL_COMPILE);
		gah=gluNewQuadric();
			//alkujuttu
			glEnable(GL_DEPTH_TEST);			
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glEnable(GL_BLEND);
			glBlendFunc(GL_SRC_ALPHA,GL_ONE_MINUS_SRC_ALPHA);
			
			//etusylinteri
			glPushMatrix();
			glTranslatef(-0.3, 1, 0);
			glColor4f(0.7,0.7,0.7,1.0f);		
			glRotatef(-180.0f,1.0f,0.0f,1.0f);	
			gluCylinder(gah, 0.1, 0.1, 0.6, 5+4*sets->detail, 1+1*sets->detail);
			glPopMatrix();

			//takasylinteri
			glPushMatrix();
			glTranslatef(-0.45, -1, 0);
			glColor4f(0.7,0.7,0.7,1.0f);		
			glRotatef(-180.0f,1.0f,0.0f,1.0f);	
			gluCylinder(gah, 0.1, 0.1, 0.9, 5+4*sets->detail, 1+1*sets->detail);
			glPopMatrix();

			//vasen sylinteri
			glPushMatrix();
			glTranslatef(-1, -1, 0);
			glColor4f(0.7,0.7,0.7,1.0f);		
			glRotatef(-180.0f,1.0f,0.0f,1.0f);	
			glRotatef( 82.5f,1,0,0);
			glTranslatef(0, 0, 0.55);
			gluCylinder(gah, 0.1, 0.1, 2.02-0.55-0.45, 5+4*sets->detail, 1+1*sets->detail);
			glPopMatrix();

			//oikea sylinteri
			glPushMatrix();
			glTranslatef( 1, -1, 0);
			glColor4f(0.7,0.7,0.7,1.0f);		
			glRotatef(-180.0f,1.0f,0.0f,1.0f);	
			glRotatef( 97.5f,1,0,0);
			glTranslatef(0, 0, 0.55);
			gluCylinder(gah, 0.1, 0.1, 2.02-0.55-0.45, 5+4*sets->detail, 1+1*sets->detail);
			glPopMatrix();

			//etupallot
			glPushMatrix();
			glTranslatef(-0.75, 1, 0);
			glColor4f(0.8, 0.1, 0.1,0.7);
			gluSphere(gah, 0.5, 15+10*sets->detail, 8+8*sets->detail);
			glTranslatef(0.75*2, 0, 0);
			gluSphere(gah, 0.5, 15+10*sets->detail, 8+8*sets->detail);
			glPopMatrix();

			//takapallot
			glPushMatrix();
			glTranslatef(-1, -1, 0);
			glColor4f(0.1, 0.1, 0.8,0.7);
			gluSphere(gah, 0.6, 15+10*sets->detail, 8+8*sets->detail);
			glTranslatef(1*2, 0, 0);
			gluSphere(gah, 0.6, 15+10*sets->detail, 8+8*sets->detail);
			glPopMatrix();

			//loppujuttu
			glEnable(GL_TEXTURE_2D);
			glDisable(GL_DEPTH_TEST);			
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);


			gluDeleteQuadric(gah);
	glEndList();
}

void alus18(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_18, GL_COMPILE);
		gah=gluNewQuadric();
			glEnable(GL_DEPTH_TEST);			
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glEnable(GL_BLEND);
			glBlendFunc(GL_SRC_COLOR,GL_ONE);

			//eka pallo
			glPushMatrix();
			glTranslatef(0, -1.3, 0);
			glColor3f(0.8, 0.1, 0.1);
			gluSphere(gah, 0.5, 10+10*sets->detail, 8+10*sets->detail);
			glPopMatrix();

			//toka pallo
			glPushMatrix();
			glTranslatef(0, 1.3, 0);
			glColor3f(0.8, 0.1, 0.1);
			gluSphere(gah, 0.5, 10+10*sets->detail, 8+10*sets->detail);
			glPopMatrix();

			//kolmas pallo
			glPushMatrix();
			glTranslatef(0.45, 0, 0);
			glColor3f(0.8, 0.1, 0.1);
			gluSphere(gah, 0.3, 10+10*sets->detail, 8+10*sets->detail);
			glPopMatrix();

			//neljäs pallo
			glPushMatrix();
			glTranslatef(-0.45, 0, 0);
			glColor3f(0.8, 0.1, 0.1);
			gluSphere(gah, 0.3, 10+10*sets->detail, 8+10*sets->detail);
			glPopMatrix();

			//sylinteri
			glPushMatrix();
			glTranslatef(0, -1, 0);
			glColor4f(0.7,0.7,0.7,0.7f);		
			glRotatef(-90.0f,1.0f,0.0f,0.0f);	
			gluCylinder(gah, 0.1, 0.1, 2, 5+5*sets->detail, 1);
			glPopMatrix();



		gluDeleteQuadric(gah);
	glEndList();
	glNewList(DRAW_ALUS_18_SALAMA, GL_COMPILE);
		float ekoko=0.7;
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*ekoko, -1.0f*ekoko,  0.0f);
			glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*ekoko, -1.0f*ekoko,  0.0f);
			glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*ekoko,  1.0f*ekoko,  0.0f);
			glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*ekoko,  1.0f*ekoko,  0.0f);
		glEnd();
	glEndList();
}

void alus19(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_19, GL_COMPILE);
		gah=gluNewQuadric();
			glEnable(GL_LIGHTING);
			glEnable(GL_DEPTH_TEST);			
			glDisable(GL_TEXTURE_2D);
			glDisable(GL_BLEND);

			glColor3f(0.3, 0.1, 0.8);

			//keskuspallo
			glPushMatrix();
			glScalef(1,1,0.5);
			gluSphere(gah, 1, 20+15*sets->detail, 10+10*sets->detail);
			glPopMatrix();

			//silmät
			glEnable(GL_BLEND);
			glBlendFunc(GL_SRC_ALPHA,GL_ONE_MINUS_SRC_ALPHA);
			glColor4f(0.6, 0.2, 0.2,0.6);
			glPushMatrix();
			glTranslatef(0.35, 0.65, 0.20);
			gluSphere(gah, 0.2, 15+8*sets->detail, 8+6*sets->detail);
			glTranslatef(-0.35*2,0, 0);
			gluSphere(gah, 0.2, 15+8*sets->detail, 8+6*sets->detail);
			glPopMatrix();
			glDisable(GL_BLEND);

			//aseet
			glPushMatrix();
				glRotatef(-90, 1, 0, 0);
				glTranslatef(-0.7, 0, 0.35);
				glColor3f(0.4, 0.3, 1);
				gluSphere(gah, 0.33, 15+8*sets->detail, 8+8*sets->detail);
				glColor3f(0.7, 0.5, 1);
				gluCylinder(gah, 0.13, 0.07, 1, 5+5*sets->detail, 1+2*sets->detail);
				glTranslatef(0.7*2, 0, 0);
				glColor3f(0.4, 0.3, 1);
				gluSphere(gah, 0.33, 15+8*sets->detail, 8+8*sets->detail);
				glColor3f(0.7, 0.5, 1);
				gluCylinder(gah, 0.13, 0.07, 1, 5+5*sets->detail, 1+2*sets->detail);
			glPopMatrix();		

		gluDeleteQuadric(gah);
	glEndList();
}

void alus20(){
	GLUquadricObj *gah;
	GLUquadricObj *gah3;
	glNewList(DRAW_ALUS_20, GL_COMPILE);
		gah=gluNewQuadric();
		gah3=gluNewQuadric();

			glEnable(GL_LIGHTING);
			glEnable(GL_TEXTURE_2D);
			glEnable(GL_DEPTH_TEST);			
			glDisable(GL_BLEND);

			gluQuadricTexture(gah3,true);
			gluQuadricDrawStyle(gah3, GLU_FILL); 
			gluQuadricNormals(gah3,GLU_SMOOTH); 

			//keskuspallo
			glPushMatrix();
			glColor3f(0.3, 0.8, 0.3);
			glScalef(1,1,0.5);
			gluSphere(gah3, 1, 10+6*sets->detail, 6+4*sets->detail);
			glPopMatrix();
			glDisable(GL_TEXTURE_2D);

			//aseet
			glPushMatrix();
			glTranslatef(0, 0.9, 0);
			glColor3f(0.33, 0.7, 0.33);
			gluSphere(gah, 0.45, 6+6*sets->detail, 5+4*sets->detail);
			glColor3f(0.37, 0.6, 0.37);
			glTranslatef(0, 0.1, 0);
			glRotatef(-90, 1, 0, 0);
			gluCylinder(gah, 0.35, 0.25, 1, 5+4*sets->detail, 1+2*sets->detail);
			glPopMatrix();

			//lehdet
			glColor3f(0.37, 0.8, 0.37);
			glRotatef( 45, 0, 0, 1);
			int i;
			for(i=0;i<4;i++){
				glTranslatef(0, 1.5, 0);
				glScalef(1,2,0.25);
				gluSphere(gah, 0.5, 12+8*sets->detail, 8+6*sets->detail);
				glScalef(1,0.5,4);
				glTranslatef(0, -1.5, 0);
				glRotatef( 90, 0, 0, 1);

			}

			glEnable(GL_TEXTURE_2D);
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);

		gluDeleteQuadric(gah3);
		gluDeleteQuadric(gah);
	glEndList();
}

void alus21(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_21_NORMAL, GL_COMPILE);
		gah=gluNewQuadric();
				glEnable(GL_LIGHTING);
				glDisable(GL_TEXTURE_2D);
				glDisable(GL_BLEND);
				glEnable(GL_DEPTH_TEST);			

			//keppi
			glPushMatrix();
			glTranslatef(0, -1, 0);
			glColor3f(0.6, 0.50, 0.5);
			glRotatef(-90, 1, 0, 0);
			gluCylinder(gah, 0.15, 0.15, 2, 7+6*sets->detail, 1+1*sets->detail);
			glPopMatrix();
			
			//kepin päät
			glPushMatrix();
			glColor3f(0.6, 0.50, 0.5);
			glTranslatef(0, -1, 0);
			glRotatef( 90, 1, 0, 0);
			gluDisk(gah, 0,0.15,7+6*sets->detail,1+1*sets->detail);

			glRotatef(-90, 1, 0, 0);
			glTranslatef(0, 2, 0);
			glRotatef(-90, 1, 0, 0);
			gluDisk(gah, 0,0.15,7+6*sets->detail,1+1*sets->detail);
			glPopMatrix();

			//tuntosarvi1
			glPushMatrix();
			glTranslatef(-0.05, 1, 0);
			glColor3f(0.6, 0.50, 0.5);
			glRotatef( 30, 1, 0, 1);
			glRotatef(-90, 1, 0, 0);
			gluCylinder(gah, 0.05, 0.05, 0.5, 5+4*sets->detail, 1+1*sets->detail);
			glTranslatef(0.0, 0.0, 0.6);
			glColor3f(0.7, 0.20, 0.2);
			gluSphere(gah, 0.2, 10+6*sets->detail, 6+5*sets->detail);
			glPopMatrix();

			//tuntosarvi2
			glPushMatrix();
			glColor3f(0.6, 0.50, 0.5);
			glTranslatef(0.05, 1, 0);
			glRotatef( 30, 1, 0, 0);
			glRotatef( -30, 0, 0, 1);
			glRotatef(-90, 1, 0, 0);
			gluCylinder(gah, 0.05, 0.05, 0.5, 5+4*sets->detail, 1+1*sets->detail);
			glTranslatef(0.0, 0.0, 0.6);
			glColor3f(0.7, 0.20, 0.2);
			gluSphere(gah, 0.2, 10+6*sets->detail, 6+5*sets->detail);
			glPopMatrix();

			//lehdet
			glPushMatrix();
			glTranslatef(0.66, 0.25, 0);
			glColor3f(0.2, 0.8, 0.2);
			glScalef(1.5,1,0.15);
			gluSphere(gah, 0.3, 10+8*sets->detail, 6+8*sets->detail);
			glTranslatef(0, -1, 0);
			gluSphere(gah, 0.3, 10+8*sets->detail, 6+8*sets->detail);
			glTranslatef(-0.88, 0.5, 0);
			gluSphere(gah, 0.3, 10+8*sets->detail, 6+8*sets->detail);
			glTranslatef(0, 1, 0);
			gluSphere(gah, 0.3, 10+8*sets->detail, 6+8*sets->detail);
			glPopMatrix();

			glEnable(GL_TEXTURE_2D);
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
			glDisable(GL_DEPTH_TEST);			

		gluDeleteQuadric(gah);
	glEndList();
	glNewList(DRAW_ALUS_21_SHADOW, GL_COMPILE);
		gah=gluNewQuadric();

			//keppi
			glPushMatrix();
			glTranslatef(0, -1, 0);
			glRotatef(-90, 1, 0, 0);
			gluCylinder(gah, 0.15, 0.15, 2, 7+6*sets->detail, 1+1*sets->detail);
			glPopMatrix();
			
			//kepin päät
			glPushMatrix();
			glTranslatef(0, -1, 0);
			glRotatef( 90, 1, 0, 0);
			gluDisk(gah, 0,0.15,7+6*sets->detail,1+1*sets->detail);

			glRotatef(-90, 1, 0, 0);
			glTranslatef(0, 2, 0);
			glRotatef(-90, 1, 0, 0);
			gluDisk(gah, 0,0.15,7+6*sets->detail,1+1*sets->detail);
			glPopMatrix();

			//tuntosarvi1
			glPushMatrix();
			glTranslatef(-0.05, 1, 0);
			glRotatef( 30, 1, 0, 1);
			glRotatef(-90, 1, 0, 0);
			gluCylinder(gah, 0.05, 0.05, 0.5, 5+4*sets->detail, 1+1*sets->detail);
			glTranslatef(0.0, 0.0, 0.6);
			gluSphere(gah, 0.2, 10+6*sets->detail, 6+5*sets->detail);
			glPopMatrix();

			//tuntosarvi2
			glPushMatrix();
			glTranslatef(0.05, 1, 0);
			glRotatef( 30, 1, 0, 0);
			glRotatef( -30, 0, 0, 1);
			glRotatef(-90, 1, 0, 0);
			gluCylinder(gah, 0.05, 0.05, 0.5, 5+4*sets->detail, 1+1*sets->detail);
			glTranslatef(0.0, 0.0, 0.6);
			gluSphere(gah, 0.2, 10+6*sets->detail, 6+5*sets->detail);
			glPopMatrix();

			//lehdet
			glPushMatrix();
			glTranslatef(0.66, 0.25, 0);
			glScalef(1.5,1,0.15);
			gluSphere(gah, 0.3, 10+8*sets->detail, 6+8*sets->detail);
			glTranslatef(0, -1, 0);
			gluSphere(gah, 0.3, 10+8*sets->detail, 6+8*sets->detail);
			glTranslatef(-0.88, 0.5, 0);
			gluSphere(gah, 0.3, 10+8*sets->detail, 6+8*sets->detail);
			glTranslatef(0, 1, 0);
			gluSphere(gah, 0.3, 10+8*sets->detail, 6+8*sets->detail);
			glPopMatrix();

		gluDeleteQuadric(gah);
	glEndList();
}

void alus22(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_22, GL_COMPILE);
		gah=gluNewQuadric();

			//alkujuttu
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glEnable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);			
			glBlendFunc(GL_SRC_ALPHA,GL_ONE_MINUS_SRC_ALPHA);

			glPushMatrix();

				//eka pallo
				glColor3f(0.3, 0.8, 0.3);
				glTranslatef(0, 1.5, 0);
				gluSphere(gah, 0.5, 10+8*sets->detail, 6+8*sets->detail);
				glPushMatrix();

				//silmät..
				glColor3f(0.8, 0.1, 0.1);
				glTranslatef(0.25,0.25,0.25);
				gluSphere(gah, 0.2, 7+5*sets->detail, 5+4*sets->detail);	

				glTranslatef(-0.25*2,0,0);
				gluSphere(gah, 0.2, 7+5*sets->detail, 5+4*sets->detail);

				glTranslatef(0,0,-2*0.25);
				gluSphere(gah, 0.2, 7+5*sets->detail, 5+4*sets->detail);

				glTranslatef(2*0.25,0,0);
				gluSphere(gah, 0.2, 7+5*sets->detail, 5+4*sets->detail);
			glPopMatrix();

		gluDeleteQuadric(gah);
	glEndList();
}

void alus23(){
	GLUquadricObj *gah;
	glNewList(DRAW_ALUS_23, GL_COMPILE);
		gah=gluNewQuadric();


		

			//kaasutus (jatkoa muualta) (alkujuttu välissä)
				//alkujuttu
				glEnable(GL_LIGHTING);
				glDisable(GL_TEXTURE_2D);
				glDisable(GL_BLEND);
				glEnable(GL_DEPTH_TEST);	

				glColor3f(0.8, 0.1, 0.1);
				gluDisk(gah, 0,0.35,8+8*sets->detail,1+1*sets->detail);
				glTranslatef(-1.2, 0, 0);
				gluDisk(gah, 0,0.35,8+8*sets->detail,1+1*sets->detail);
			glPopMatrix();

			//runko
			glPushMatrix();
				glColor3f(0.7, 0.7, 0.7);
				glTranslatef(-0.6, -1.5, -0.35);
				glRotatef(-90, 1, 0, 0);
				gluCylinder(gah, 0.35, 0.35, 3, 8+8*sets->detail, 1+1*sets->detail);
				glTranslatef(1.2, 0, 0);
				gluCylinder(gah, 0.35, 0.35, 3, 8+8*sets->detail, 1+1*sets->detail);
			glPopMatrix();

			glPushMatrix();
				glColor3f(0.7, 0.85, 0.7);
				glScalef(0.9,1.2,0.75);
				gluSphere(gah, 1, 18+8*sets->detail, 10+8*sets->detail);
			glPopMatrix();
			
			//aseet
			glPushMatrix();
				glColor3f(0.65, 0.73, 0.65);
				glTranslatef(0, 1, 0.3);
				gluSphere(gah, 0.3, 18+8*sets->detail, 10+8*sets->detail);
				glRotatef(-90, 1, 0, 0);
				glColor3f(0.65, 0.68, 0.65);
				gluCylinder(gah, 0.2, 0.15, 0.6, 7+7*sets->detail, 1+1*sets->detail);
			glPopMatrix();

			glEnable(GL_TEXTURE_2D);
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
			glDisable(GL_DEPTH_TEST);			

		gluDeleteQuadric(gah);
	glEndList();
}

/*
void alus7(){
	GLUquadricObj *gah;
	GLUquadricObj *gah3;
	glNewList(DRAW_ALUS_12_OSA_1, GL_COMPILE);
		gah=gluNewQuadric();
		gluDeleteQuadric(gah);
	glEndList();
}

*/

void alusPiirto(){
	alus0();
	alus1();
	alus2();
	alus3();
	alus4();
	alus5();
	alus6();
	alus7();
	alus8();
	alus9();
	alus10();
	alus11();
	alus12();
	alus13();
	alus14();
	alus15();
	alus16();
	alus17();
	alus18();
	alus19();
	alus20();
	alus21();
	alus22();
	alus23();

}	
