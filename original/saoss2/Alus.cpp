#include "Alus.h"
#include "main.h"
#include "net.h"
#include "ctf.h"
/*#define DRAW_CUBE list_int[0]
#define DRAW_TRI list_int[1]
#define DRAW_SPHERE_1_20_20 list_int[2]
#define	DRAW_SPHERE_05_10_5 list_int[3]
#define DRAW_ASEET_1 list_int[4]*/

Alus::Alus(){
	ampumisnopeus=10;
	patiaika=100;
	maxpadit=5;
	patinopeus=0.6;
	patisade=0.2;
	ampumisaani=NULL;
	osumisaani=NULL;
}
Alus::~Alus(){
}
void Alus::piirra(float x, float y, float z, float kulma, float asekulma, int numero2){
//Alus::piirra(double x, double y, double kulma, double asekulma, int numero){
	//if(aselaji==1){
//	double silmakoko, putkil, silmax, putkip;
	double ekoko;
	bool kaasutus=false;
	if((!joinpeli && player[numero2].pylos) || (joinpeli && player[numero2].kaasu))
		kaasutus=true;
	
	glDisable(GL_CULL_FACE);
	glPushMatrix();
	glTranslatef(x,y,z);
	GLUquadricObj *gah=gluNewQuadric();
	GLUquadricObj *gah2=gluNewQuadric();
	GLUquadricObj *gah3=gluNewQuadric();
	gluQuadricNormals(gah3,GLU_SMOOTH); 
	gluQuadricNormals(gah2,GLU_SMOOTH); 
	gluQuadricNormals(gah,GLU_SMOOTH); 
	if(!debugmode){
		float tkoko=3.0f*sade;
		glDisable(GL_LIGHTING);
		glEnable(GL_TEXTURE_2D);
		glEnable(GL_BLEND);
		glDisable(GL_DEPTH_TEST);	
		valitseVari(player[numero2].tiimi);
//		float syvyys=sade*-0.5;
		glBlendFunc(GL_SRC_ALPHA,GL_ONE_MINUS_SRC_COLOR);
		glBindTexture(GL_TEXTURE_2D, tiimitexture);
		float syvyys=-0.2f;
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*tkoko, -1.0f*tkoko, syvyys);
			glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*tkoko, -1.0f*tkoko, syvyys);
			glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*tkoko,  1.0f*tkoko, syvyys);
			glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*tkoko,  1.0f*tkoko, syvyys);
		glEnd();
		if(hostpeli || joinpeli){
			glPushMatrix();
				glRotatef(kulma-90.0f, 0, 0, 1);
				gl_println( (-0.6*0.8)*strlen(netnick[numero2]), -1*tkoko/1.5f,	0, netnick[numero2],	0.8);
			glPopMatrix();
			if(gamemode==CTF){
				int i;
				for(i=0;i<cpisteita;i++){
					if(cpiste[i].enabled && cpiste[i].player==numero2){
						glEnable(GL_LIGHTING);
						glDisable(GL_TEXTURE_2D);
						glDisable(GL_BLEND);
						glEnable(GL_DEPTH_TEST);	
						glPushMatrix();
							valitseVari(i);
							float koko=sade/2.0f+2;
							glScalef(koko,koko,koko);
							glRotatef(90,1,0,0);
							glRotatef(kulma,0,1,0);
							glCallList(KUKKA4);
						glPopMatrix();
					}
				}
			}
		}
	}
//			printf("piirto %i\n",numero);
	switch(numero){
		case 0:
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glDisable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);			

			//alkupyörittely
			glPushMatrix();
			glScalef(sade/1.7f,sade/1.7f,sade/1.7f);
			glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	
//			glTranslatef(0,-1.2,0);


			//runko
			glPushMatrix();
				if(player[numero2].suojapaalla) glColor3f(0.5, 0.8, 0.9);
				else glColor3f(0.9, 0.8, 0.5);
				glCallList(DRAW_ALUS_0_OSA_1);
				if(player[numero2].suojapaalla) glColor4f(0.8, 0.5, 0.2,0.5);
				else  glColor4f(0.2, 0.5, 0.8,0.5);
				glCallList(DRAW_ALUS_0_OSA_2);
			glPopMatrix();

			//takasiivekkeet
			glPushMatrix();
				if(player[numero2].suojapaalla) glColor3f(0.4, 0.6, 0.75);
				else glColor3f(0.75, 0.6, 0.4);
				glTranslatef(0, -0.9, 0.2);
				glScalef(1.5, 0.35, 0.02);
				glCallList(DRAW_CUBE);
				glTranslatef(0, 0, -0.4*50);
				glCallList(DRAW_CUBE);
			glPopMatrix();
			
			//aseet
			glPushMatrix();
				glTranslatef(1.3, -0.9, 0);
				glScalef(0.7,1,1);
				if(player[numero2].suojapaalla) glColor3f(0.5, 0.55, 0.65);
				else glColor3f(0.65, 0.55, 0.5);
				glCallList(DRAW_ALUS_0_OSA_3);
			glPopMatrix();

			//lopetus
			glPopMatrix();
			glEnable(GL_TEXTURE_2D);
			glDisable(GL_DEPTH_TEST);			
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
			break;

		case 1:
			glPushMatrix();

			//pyörittely
				glScalef(sade/1.0f*0.7f,sade/1.0f*0.7f,sade/1.0f*0.7f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	

				//piirretään alus
				glCallList(DRAW_ALUS_1);
			glPopMatrix();
			break;

		case 2:
			glPushMatrix();

			//pyörittely
				glScalef(sade/1.0f*0.6f,sade/1.0f*0.6f,sade/1.0f*0.7f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	

				//piirretään alus
				glCallList(DRAW_ALUS_2);
			glPopMatrix();
			break;

		case 3:
			glPushMatrix();

			//pyörittely
				glScalef(sade/1.7f,sade/1.7f,sade/1.7f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	

				//piirretään alus
				glCallList(DRAW_ALUS_3);
			glPopMatrix();
			break;
		case 4:

			glPushMatrix();
				//skeilataan ja pyöritetään kohdalleen
				glScalef(sade/1.0f*0.6f,sade/1.0f*0.6f,sade/1.0f*0.7f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	
			
				//piirretään ensimmäinen osa
				glCallList(DRAW_ALUS_4_OSA_1);

				//piirretään propellit
				int i;
				for (i = 0; i < 3; i++){
					glPushMatrix();
					glRotatef(player[numero2].propelli + i*120, 0, 0, 1);
					glRotatef(5, 0, 1, 0);
					glScalef(0.02, 0.3, 0.2);
					glTranslatef(0, 1, 3.5);
					glCallList(DRAW_CUBE);
				glPopMatrix();
			}

				//piirretään kaasutusjutut
			glPushMatrix();
				//eka kaasutusdiski
				if(kaasutus){
					glTranslatef(1, 0, 0.1);
					glColor3f(0.9, 0.1, 0.1);
					glTranslatef(0, 0, 0.7);
					gluDisk(gah, 0,0.3,5+8*sets->detail,2+2*sets->detail);
					glTranslatef(0, 0, -0.7);
				}

				//toka kaasutusdiski
				if(kaasutus){
					glTranslatef(-2, 0, 0);
					glColor3f(0.9, 0.1, 0.1);
					glTranslatef(0, 0, 0.7);
					gluDisk(gah, 0,0.3,5+8*sets->detail,2+2*sets->detail);
				}

			glPopMatrix();

			//piirretään aluksen loppuosa
			glCallList(DRAW_ALUS_4_OSA_2);
			glPopMatrix();
			break;

		case 5:
			glPushMatrix();

			//pyörittely
				glScalef(sade/1.4f,sade/1.4f,sade/1.4f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	

				//piirretään alus
				glCallList(DRAW_ALUS_5);
			glPopMatrix();
			break;

		case 6:
			glPushMatrix();

			//skeilataan ja pyöritetään kohdalleen
				glScalef(sade/1.0f*0.7f,sade/1.0f*0.7f,sade/1.0f*0.7f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	
		
				//piirretään eka osa aluksesta
				glCallList(DRAW_ALUS_6_OSA_1);

				//pyöritetään tykki paikalleen
				glRotatef(kulma-asekulma,0.0f,1.0f,0.0f);	

				//piirretään toka osa..
				glCallList(DRAW_ALUS_6_OSA_2);
			glPopMatrix();
			break;

		case 7:

			glPushMatrix();

				//pyörittely
				glScalef(sade/1.1f*0.5f,sade/1.1f*0.5f,sade/1.1f*0.5f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	

				//piirretään alus
				glCallList(DRAW_ALUS_7);

				//kaasu
				glPushMatrix();
					glTranslatef(0, -1.5, 0);
					if(kaasutus){
						glRotatef(90, 1, 0, 0);
					}else{
						glRotatef(-90, 1, 0, 0);
					}
					gluDisk(gah, 0,0.35,8+8*sets->detail,2*sets->detail);
				glPopMatrix();

			glPopMatrix();
			glEnable(GL_TEXTURE_2D);
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
			glDisable(GL_DEPTH_TEST);			
			break;
		case 8:
			glPushMatrix();

			//pyörittely
				glScalef(sade/1.1f*0.5f,sade/1.1f*0.5f,sade/1.1f*0.5f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	

				//piirretään alus
				glCallList(DRAW_ALUS_8);

				//kaasutusjuttu
				if(kaasutus){
					glColor3f(0.8, 0.1, 0.1);
					gluDisk(gah, 0,0.5,8+7*sets->detail,2+2*sets->detail);
				}
				glPopMatrix();

			glPopMatrix();
			glEnable(GL_TEXTURE_2D);
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
			glDisable(GL_DEPTH_TEST);			
			break;
		case 9:
			glPushMatrix();
				glScalef(sade/1.1f*0.7f,sade/1.1f*0.7f,sade/1.1f*0.7f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	
				glCallList(DRAW_ALUS_9);
			glPopMatrix();
			break;

		case 10:

			glPushMatrix();
				
				//pyöritellään
				glScalef(sade/1.1f*0.7f,sade/1.1f*0.7f,sade/1.1f*0.7f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	

				//piirretään alus
				glCallList(DRAW_ALUS_10);
			glPopMatrix();
			break;
		case 11:
			glPushMatrix();

			//pyöritellään
				glScalef(sade*0.8f,sade*0.8f,sade*0.8f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	
				glTranslatef(0,-0.2,0);

				//venytellään jos jaksaa..
				if(player[numero2].suojapaalla){
					glScalef(0.7,1.3,0.8);
				}

				//piirretään alus.
				glCallList(DRAW_ALUS_11);
			glPopMatrix();
			break;
		case 12:
			glPushMatrix();

			//alkupyöritys
				glScalef(sade/1.1f*0.65f,sade/1.1f*0.65f,sade/1.1f*0.65f);
				glDisable(GL_TEXTURE_2D);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	

				//piirretään aluksen runko
				glCallList(DRAW_ALUS_12_OSA_1);

				//kaasujuttu..
				if(kaasutus){
					glColor3f(0.9, 0.1, 0.1);
					gluDisk(gah, 0,0.2,8+6*sets->detail,1+2*sets->detail);
				}else{
					glColor3f(0.5, 0.1, 0.1);
					gluDisk(gah, 0,0.2,8+6*sets->detail,1+2*sets->detail);
				}

				//siivet..
				glCallList(DRAW_ALUS_12_OSA_2);

			glPopMatrix();
			break;
		
		case 13:
			glEnable(GL_LIGHTING);
			glDisable(GL_TEXTURE_2D);
			glEnable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);			

			glPushMatrix();

			//käännellään
				glScalef(sade/1.1f*0.65f,sade/1.1f*0.65f,sade/1.1f*0.65f);
				glDisable(GL_TEXTURE_2D);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	
				glBlendFunc(GL_SRC_ALPHA,GL_ONE_MINUS_SRC_ALPHA);
			
				if(player[numero2].pati[0].aika>=patiaika-3){
					ekoko=0.7*player[numero2].pati[0].r+0.2;
				}else ekoko=0.2;

				//keskuspallo
				glPushMatrix();
				glColor4f(1.0f,0.0f,0.0f,1.0f);			
				glScalef(0.7,0.5,1);
				gluSphere(gah, 0.35, 8, 5);
				glPopMatrix();

				//keskus
				glPushMatrix();
				glColor4f(ekoko,1.0f-ekoko,0.2f,0.7f);			
				glScalef(0.7,1,0.5);
				gluSphere(gah, 1, 12+8*sets->detail, 8+6*sets->detail);
				glPopMatrix();

				//piirretään loppualus
				glCallList(DRAW_ALUS_13);

				//+kaasujutut
				if(kaasutus){
					glPushMatrix();
					glColor4f(0.7f,0.2f,0.2f,0.7f);			
					glTranslatef(0,-1.8,0);
					glScalef(1,1,0.5);
					glRotatef(90, 1, 0, 0);
					gluDisk(gah, 0,0.7,10+10*sets->detail,1+2*sets->detail);
					glPopMatrix();
				}

				glEnable(GL_TEXTURE_2D);
				glDisable(GL_DEPTH_TEST);			
				glEnable(GL_BLEND);
				glDisable(GL_LIGHTING);
			glPopMatrix();
			break;

		case 14:
			glPushMatrix();

			//alkupyörittely..
				glScalef(sade*0.7f,sade*0.7f,sade*0.7f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	

				//piirretään aluksen eka osa
				glCallList(DRAW_ALUS_14_OSA_1);

				//vaihdetaan väri kaasutusdiskiä varten
				if(kaasutus){
					glColor3f(0.7f,0.2f,0.2f);			
				}else{
					glColor3f(0.6f,0.5f,0.5f);			
				}

				//piirretään aluksen toka osa
				glCallList(DRAW_ALUS_14_OSA_2);

			glPopMatrix();
			break;

		case 15:
			glPushMatrix();
			glScalef(sade/1.1f*0.65f,sade/1.1f*0.65f,sade/1.1f*0.65f);
			glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	
			if(!player[numero2].suojapaalla){
				glCallList(DRAW_ALUS_15_OSA_1);
				if(kaasutus){
					glScalef(1,1,1);
				}else{
					glScalef(1,0.5,1);
				}
				glCallList(DRAW_ALUS_15_OSA_2);
			}else{
				glBindTexture(GL_TEXTURE_2D, texture);
				glCallList(DRAW_ALUS_15_OSA_3);
			}
			glPopMatrix();

			break;
		case 16:
			glPushMatrix();

			//pyörittely
				glScalef(sade/1.1f*0.7f,sade/1.1f*0.7f,sade/1.1f*0.7f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);
			
				//piirretään suurin osa aluksesta..
				glCallList(DRAW_ALUS_16_OSA_1);
	
				//pyöritetään kieli kohdalleen
				glRotatef(sin(player[numero2].propelli/50.0f)*35, 1, 0, 0);

				//piirretään kieli
				glCallList(DRAW_ALUS_16_OSA_2);
			glPopMatrix();

			//loppujuttu
			glEnable(GL_TEXTURE_2D);
			glDisable(GL_DEPTH_TEST);			
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
			break;
		case 17:

			glPushMatrix();

			//pyöritellään..
				glScalef(sade/1.0f*0.7f,sade/1.0f*0.7f,sade/1.0f*0.7f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);

				//piirretään alus
				glCallList(DRAW_ALUS_17);

			glPopMatrix();

			break;
		case 18:
			glPushMatrix();

			//pyöritellään ja otetaan salamatekstuuri käyttöön.
				glScalef(sade/1.0f*0.7f,sade/1.0f*0.7f,sade/1.0f*0.7f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	
				glRotatef(player[numero2].propelli/5.0f,1.0f,0.0f,0.0f);	
				glRotatef(player[numero2].propelli/4.0f,0.0f,1.0f,0.0f);	
				glRotatef(player[numero2].propelli/3.0f,0.0f,0.0f,1.0f);
				glBindTexture(GL_TEXTURE_2D, texture2);

				//piirretään alus
				glCallList(DRAW_ALUS_18);

	
				//ekat salamat
				glPushMatrix();
					glTranslatef(0, -1.3, 0);
					glColor4f(2,2,2,1.5f);		
					glEnable(GL_TEXTURE_2D);
					glBlendFunc(GL_SRC_COLOR,GL_ONE);
					glRotatef(-1*player[numero2].propelli/3.0f,0.0f,0.0f,1.0f);
					glRotatef(-1*player[numero2].propelli/4.0f,0.0f,1.0f,0.0f);	
					glRotatef(-1*player[numero2].propelli/5.0f,1.0f,0.0f,0.0f);	
	/*			glRotatef(sin(player[numero2].propelli/180)*20,0,0,1);
				glRotatef(cos(player[numero2].propelli/180)*20,0,1,0);
				glRotatef(sin(player[numero2].propelli/180)*-15,1,0,0);*/
					glCallList(DRAW_ALUS_18_SALAMA);
				glPopMatrix();
			
				//tokat salamat
				glPushMatrix();
					glTranslatef(0, 1.3, 0);
					glRotatef(-1*player[numero2].propelli/3.0f,0.0f,0.0f,1.0f);
					glRotatef(-1*player[numero2].propelli/4.0f,0.0f,1.0f,0.0f);	
					glRotatef(-1*player[numero2].propelli/5.0f,1.0f,0.0f,0.0f);	
					glCallList(DRAW_ALUS_18_SALAMA);
				glPopMatrix();
		
				//kolmannet salamat
				//ekoko=0.45;
				glPushMatrix();
					glTranslatef(0.45, 0, 0);
					glScalef(0.45/0.7,0.45/0.7,0.45/0.7);
					glRotatef(-1*player[numero2].propelli/3.0f,0.0f,0.0f,1.0f);
					glRotatef(-1*player[numero2].propelli/4.0f,0.0f,1.0f,0.0f);	
					glRotatef(-1*player[numero2].propelli/5.0f,1.0f,0.0f,0.0f);	
					glCallList(DRAW_ALUS_18_SALAMA);
				glPopMatrix();

				//neljännet salamat
				//ekoko=0.45;
				glPushMatrix();
					glTranslatef(-0.45, 0, 0);
					glScalef(0.45/0.7,0.45/0.7,0.45/0.7);
					glRotatef(-1*player[numero2].propelli/3.0f,0.0f,0.0f,1.0f);
					glRotatef(-1*player[numero2].propelli/4.0f,0.0f,1.0f,0.0f);	
					glRotatef(-1*player[numero2].propelli/5.0f,1.0f,0.0f,0.0f);	
					glCallList(DRAW_ALUS_18_SALAMA);
				glPopMatrix();	


			glPopMatrix();

//		glEnable(GL_TEXTURE_2D);
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
			glDisable(GL_DEPTH_TEST);			
			break;

		case 19:
//			glBlendFunc(GL_SRC_ALPHA,GL_ONE);
			glPushMatrix();

				//pyörittely
				glScalef(sade/1.0f*0.7f,sade/1.0f*0.7f,sade/1.0f*0.7f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	

				//piirretään aluksen runko ja aseet
				glCallList(DRAW_ALUS_19);

				//eka moottori
				glPushMatrix();
				glTranslatef(0, -0.8, 0);
				glRotatef( 90.0f,1.0f,0.0f,0.0f);	
				glColor3f(0.3, 0.1, 0.8);
				gluCylinder(gah, 0.2, 0.3, 0.6, 15+8*sets->detail, 4+2*sets->detail);
				glTranslatef(0, 0, 0.6);
				if(kaasutus){
					glColor3f(0.8, 0.1, 0.1);
					gluDisk(gah, 0,0.3,15+8*sets->detail,2+1*sets->detail);
				}
				glPopMatrix();

				//toka moottori
				glPushMatrix();
				glRotatef(40.0f,0.0f,0.0f,1.0f);	
				glTranslatef(0, -0.8, 0);
				glRotatef( 90.0f,1.0f,0.0f,0.0f);	
				glColor3f(0.3, 0.1, 0.8);
				gluCylinder(gah, 0.2, 0.3, 0.6, 15+8*sets->detail, 4+2*sets->detail);
				glTranslatef(0, 0, 0.6);
				if(kaasutus){
					glColor3f(0.8, 0.1, 0.1);
					gluDisk(gah, 0,0.3,15+8*sets->detail,2+1*sets->detail);
				}
				glPopMatrix();	

				//kolmas moottori
				glPushMatrix();
				glRotatef(-40.0f,0.0f,0.0f,1.0f);	
				glTranslatef(0, -0.8, 0);
				glRotatef( 90.0f,1.0f,0.0f,0.0f);	
				glColor3f(0.3, 0.1, 0.8);
				gluCylinder(gah, 0.2, 0.3, 0.6, 15+8*sets->detail, 4+2*sets->detail);
				glTranslatef(0, 0, 0.6);
				if(kaasutus){
					glColor3f(0.8, 0.1, 0.1);
					gluDisk(gah, 0,0.3,15+8*sets->detail,2+1*sets->detail);
				}
				glPopMatrix();


				//kieli
				if(player[numero2].easeviive<easenopeus/2){
					ekoko=(player[numero2].easeviive)/20;
				}else{
					ekoko=(easenopeus-player[numero2].easeviive)/20;
				}
				glPushMatrix();
				glTranslatef(0, 0.9+ekoko/5, 0);
				glColor3f(0.8, 0.2, 0.2);
				glScalef(0.3,ekoko+0.1,0.1);
				gluSphere(gah2, 1, 12+8*sets->detail, 4);
				glPopMatrix();	


			glPopMatrix();
			glEnable(GL_TEXTURE_2D);
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
			glDisable(GL_DEPTH_TEST);			
			break;

		case 20:
			glPushMatrix();

				//pyöritellään ja otetaan tekstuuri käyttöön.
				glScalef(sade/1.0f*0.5f,sade/1.0f*0.5f,sade/1.0f*0.5f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	
				glBindTexture(GL_TEXTURE_2D, texture);

				//piirretään alus
				glCallList(DRAW_ALUS_20);

			glPopMatrix();
			break;
		case 21:
			glPushMatrix();

				//pyöritellään
				glScalef(sade/1.1f*0.7f,sade/1.1f*0.7f,sade/1.1f*0.7f);
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	

				if(!varjomode){

					//piirretään alus normaalimodessa
					glCallList(DRAW_ALUS_21_NORMAL);
				}else{

					//piirretään alus varjomodessa
					glCallList(DRAW_ALUS_21_SHADOW);
				}
			glPopMatrix();

			break;
		case 22:

			glPushMatrix();

			//pyöritellään
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	
				glScalef(sade*0.5f,sade*0.5f,sade*0.5f);

				//piirretään pää
				glCallList(DRAW_ALUS_22);

				//muut pallot
				glColor3f(0.3, 0.8, 0.3);
				for(i=1;i<palikkamaara/2;i++){
					glRotatef(player[numero2].kulmahanta[i*2]-player[numero2].kulmahanta[(i-1)*2],0,0,1);
						glTranslatef(0,-0.5
						,0);
					glColor4f(0.3, 0.8, 0.3,1.0-i*0.1);
					gluSphere(gah, 0.5-0.02*i, 10+8*sets->detail, 6+8*sets->detail);
				}
			glPopMatrix();


			glPopMatrix();
			glEnable(GL_TEXTURE_2D);
			glEnable(GL_BLEND);
			glDisable(GL_LIGHTING);
			glDisable(GL_DEPTH_TEST);			
			break;
		case 23:
			glPushMatrix();


				//pyöritellään.
				glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);	
				glScalef(sade*0.5f,sade*0.5f,sade*0.5f);


				//kaasutuskääntö (matriisi popataan listassa)
				glPushMatrix();
					glTranslatef( 0.6, -1.5, -0.35);
					glRotatef(-90, 1, 0, 0);
					if(kaasutus){
						glRotatef(-180, 1, 0, 0);
					}
				
				//piirretään kaasutus loppuun ja muukin runko..
				glCallList(DRAW_ALUS_23);

			glPopMatrix();
			break;

		default:
			glDisable(GL_LIGHTING);
			glEnable(GL_TEXTURE_2D);
			glEnable(GL_BLEND);
			glEnable(GL_DEPTH_TEST);
			glBlendFunc(GL_SRC_ALPHA,GL_ONE_MINUS_SRC_COLOR);
			if((numero==0 || numero==11 || numero==15) && player[numero2].suojapaalla) glBindTexture(GL_TEXTURE_2D, texture2);
			else glBindTexture(GL_TEXTURE_2D, texture);
			glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);
				glColor4f(1.0f,1.0f,1.0f,1.0f);		

			glBegin(GL_QUADS);
				glNormal3f(0,0,1.0f);
				glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*sade, -1.0f*sade,  0.0f);	// Bottom Left Of The Texture and Quad
				glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*sade, -1.0f*sade,  0.0f);	// Bottom Right Of The Texture and Quad
				glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*sade,  1.0f*sade,  0.0f);	// Top Right Of The Texture and Quad
				glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*sade,  1.0f*sade,  0.0f);	// Top Left Of The Texture and Quad
			glEnd();

			break;
	}

	if(gamemode==CTF){
		int i;
		for(i=0;i<cpisteita;i++){
			if(cpiste[i].enabled && cpiste[i].player==numero2){
				glEnable(GL_LIGHTING);
				glDisable(GL_TEXTURE_2D);
				glDisable(GL_BLEND);
				glEnable(GL_DEPTH_TEST);	
				glPushMatrix();
					valitseVari(i);
					float koko=sade/2.0f+2;
					glScalef(koko,koko,koko);
					glRotatef(90,1,0,0);
					glRotatef(kulma,0,1,0);
					glCallList(KUKKA4);
				glPopMatrix();
			}
		}
	}

	gluDeleteQuadric(gah);
	gluDeleteQuadric(gah2);
	gluDeleteQuadric(gah3);
	glPopMatrix();
	glEnable(GL_CULL_FACE);
}
