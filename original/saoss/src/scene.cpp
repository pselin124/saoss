#include "scene.h"
#include "main.h"
#include "teksti.h"
#include "Define.h"
#include "Star.h"
#include "net.h"
#include "domination.h"
#include "ctf.h"
#include "sieni.h"

#include <SDL.h>
#include <GL/gl.h>
#include <GL/glu.h>


void calculateFrame(int framecount){
//	if(musiikkipaalla && ekakerta==1) musicDone();
	int i,tempi;
	double cx, cy, cz, tx, ty, tz,t2x,t2y;
	double kulma, xy, px, py, kulma2, kulma3, pv;

	//liikutetaan ruutua
GLfloat lightdiffuse[4]= {0.8, 0.8, 0.8, 0.8};
glLightfv(GL_LIGHT0,GL_DIFFUSE, lightdiffuse);

	cx=camposx;
	cy=camposy;
	cz=camposz;
	tx=minx;ty=miny;
	t2x=maxx;t2y=maxy;
	for(i=0;i<players;i++){
		if(player[i].pisteet>=0 || !pistepeli){
			if(player[i].x>tx)tx=player[i].x;
			if(player[i].y>ty)ty=player[i].y;
			if(player[i].x<t2x)t2x=player[i].x;
			if(player[i].y<t2y)t2y=player[i].y;
		}
	}
	tx=(tx+t2x)/2.0f;
	ty=(ty+t2y)/2.0f;
	//tx=(player[0].x+player[1].x)/2;
	//ty=(player[0].y+player[1].y)/2;
	camposx=(19*cx+tx/1.2f)/20.0f;
	camposy=(19*cy+ty/1.2f)/20.0f;

	tx=0;ty=0;
	for(i=0;i<players-1;i++){
		if(player[i].pisteet>=0 || !pistepeli){
			for(tempi=i;tempi<players;tempi++){
				if(player[tempi].pisteet>=0 ||!pistepeli){
					if(fabs(player[i].x-player[tempi].x)>tx) tx=fabs(player[i].x-player[tempi].x);
					if(fabs(player[i].y-player[tempi].y)>ty) ty=fabs(player[i].y-player[tempi].y);
				}
			}
		}
	}
//	tx=fabs(player[0].x-player[1].x);
//	ty=fabs(player[0].y-player[1].y);
	//if(fabs(tx)>fabs(ty*64.0f/48.0f)) tz=fabs(tx)*1.15;
	if(sets->cammode==0){
		if(fabs(tx)>fabs(ty*64.0f/64.0f)) tz=fabs(tx);
		else tz=fabs(ty);
		camposz=(camposz*19+(tz*(4/3.0f)))/20.0f;
	}else{
		if(fabs(tx)>fabs(ty*48.0f/64.0f)) tz=fabs(tx);
		else tz=fabs(ty);
		camposz=(camposz*19+(tz*(4/3.0f)))/20.0f;
	}

	float y=3.0f/4.0f*camposy;
	float x=3.0f/4.0f*camposx;
	float z=camposz-alustaso;
	float xjay=sqrt(x*x+y*y);
	if(sets->cammode==0){
		alfa=atan2(y,x)*180.0f/3.14;
		alfa2=((xjay/50.0f)*atan2(y,x)*180.0f/3.14+250.0f*alfa2)/(250.0f+xjay/50.0f);
		beta2=-atan2(xjay,z)*180.0f/3.14;
		beta=(-atan2(xjay,z)*180.0f/3.14+19.0f*beta)/20.0f;
	}else{
		alfa=0;
		beta=0;
		beta2=0;
		alfa2=0;
	}

	/*gluLookAt(camposx/4,camposy/4,camposz,
	camposx/1,camposy/1,alustaso,
	cos(alfa/180.0f*3.14f),sin(alfa/180.0f*3.14f),0);
*/


	//liikutetaan kaikki tarpeellinen
	if(!joinpeli) for(i=0;i<players;i++){
		if(player[i].pisteet>=0 || !pistepeli){
			if(player[i].ihmispelaaja==false) player[i].ailiiku();
		}
	}

	for(i=0;i<players;i++){
		if(player[i].pisteet>=0 || !pistepeli){
			if(player[i].kuolema<=0){
				player[i].liiku();
				player[i].laskeKaasujaljet();
				player[i].liikutaPadit();
				player[i].liikutaePadit();
				if(!joinpeli){
					player[i].tarkistaHajosikoPadit();
				}
			}
		}
	}

	for(i=0;i<players;i++){
		if(player[i].pisteet>=0 || !pistepeli){
			if(player[i].kuolema>0 && player[i].kuolema<syntymisaika) player[i].laskeAlkuSirpaleet();
		}
	}

	laskePikkumiehet();
	laskeAsteroidit();
	
	//planeetan vetovoima+tˆrm‰ys
	if(!joinpeli)for(i=0;i<players;i++){
		if(player[i].pisteet>=0 || !pistepeli){
			kulma=atan2(player[i].y,player[i].x);
			kulma2=atan2(player[i].yvauhti,player[i].xvauhti);
			px=player[i].x;
			py=player[i].y;
			pv=sqrt(player[i].xvauhti*player[i].xvauhti+player[i].yvauhti*player[i].yvauhti);
			xy=sqrt(px*px+py*py);
			if((px!=0 || py!=0) && player[i].alus.aselaji!=6 && player[i].alus.aselaji!=18 && player[i].alus.numero!=22){
	//			if(xy<15){
					player[i].xvauhti-=player[i].x/(pow(xy,2.3))*gravity*(pmassa/player[i].alus.paino);
					player[i].yvauhti-=player[i].y/(pow(xy,2.3))*gravity*(pmassa/player[i].alus.paino);
	//			}else{
	//				player[i].xvauhti-=player[i].x/(xy*xy)*gravity/5.0f*(pmassa/player[i].alus.paino);
	//				player[i].yvauhti-=player[i].y/(xy*xy)*gravity/5.0f*(pmassa/player[i].alus.paino);
	//			}
			}
			//tˆrm‰ys
			if(xy<planeettasade+player[i].alus.sade && player[i].alus.aselaji!=6 && player[i].alus.aselaji!=18 && player[i].alus.numero!=22){
				if(pv<0.1) pv=0.1;
				player[i].energia-=planeettadamage;
				kulma3=kulma+(kulma-(kulma2+PI));
				player[i].xvauhti=cos(kulma3)*pv;
				player[i].yvauhti=sin(kulma3)*pv;
				px=player[i].x+player[i].xvauhti;
				py=player[i].y+player[i].yvauhti;
				xy=sqrt(px*px+py*py);
				if(xy<5){
					player[i].xvauhti*=-1;
					player[i].yvauhti*=-1;
				}
			}else if(xy<planeettasade+player[i].alus.sade){
				player[i].energia-=planeettadamage;
				player[i].x+=player[i].x*player[i].alus.nopeus*2;
				player[i].y+=player[i].y*player[i].alus.nopeus*2;
			}
		}
	}
	if(pistepeli && !saavalita && ekapisteframe){
		for(i=0;i<players;i++){
			int temp3=random(aluksia);
			while(player[i].pisteet<alus[temp3].pistemaara){
				temp3=random(aluksia);
			}
			player[i].uudestaan(alus[temp3]);
			player[i].pisteet-=alus[temp3].pistemaara;
		}
	}
	else if(pistepeli && ekapisteframe){
		for(i=0;i<players;i++){
			valitseAlus(i);
		}
	}
	ekapisteframe=false;

	//kuoleminen->sirpaleet, uusi alus
	if(!joinpeli) for(i=0;i<players;i++){
		if(player[i].pisteet>=0 || !pistepeli){
			if(hostpeli){
				if(player[i].kuolema>0 && player[i].kuolema!=syntymisaika){
					player[i].kuolema--;
				}
			}
			else if(player[i].kuolema>0)player[i].kuolema--;
		}
	}

	if(joinpeli){
		for(i=0;i<players;i++){
			if(playerrajahtaa[i]){
				if(efektitpaalla){
					if(player[i].laseraani) player[i].ampumisaani->stop();
					player[i].laseraani=false;
					if(alus[2].eampumisaani!=NULL) alus[2].eampumisaani->play();
				}
				playerrajahtaa[i]=false;
				int j=0,scount=200+sets->detail*100;
				while(j<sirpaleita && scount>0){
					if(sirpale[j].aika<=0){
						scount--;
						sirpale[j].x=prajahdys[i].x;
						sirpale[j].y=prajahdys[i].y;
						sirpale[j].z=0;
						sirpale[j].xv=random(1500)/1000.0f-0.75f;
						sirpale[j].yv=random(1500)/1000.0f-0.75f;
						sirpale[j].zv=random(1500)/1000.0f-0.75f;
						while(sqrt(sirpale[j].xv*sirpale[j].xv+sirpale[j].yv*sirpale[j].yv+sirpale[j].zv*sirpale[j].zv)>0.64f){
							sirpale[j].xv=random(1500)/1000.0f-0.75f;
							sirpale[j].yv=random(1500)/1000.0f-0.75f;
							sirpale[j].zv=random(1500)/1000.0f-0.75f;
						}
						sirpale[j].maxaika=400;
						sirpale[j].aika=random(200)+200;
						sirpale[j].koko=random(200)/100.0f+0.6f;
						sirpale[j].muoto=0;
						sirpale[j].red=random(50)/100.0f+0.5f;
						sirpale[j].green=random(100)/100.0f;
						sirpale[j].blue=random(100)/100.0f;
					}
					j++;
				}	
			}
		}
	}

	for(i=0;i<players;i++){
		if(player[i].pisteet>=0 || !pistepeli){
			if(player[i].energia<=0 && player[i].kuolema<=0){ 
	//			if(player[i].laseraani){
	//			}
				int scount=200+sets->detail*100;
				int j=0;
				if(hostpeli){
					playerrajahtaa[i]=true;
					prajahdys[i].x=player[i].x;
					prajahdys[i].y=player[i].y;
				}
				if(!joinpeli) while(j<sirpaleita && scount>0){
					if(sirpale[j].aika<=0){
						scount--;
						sirpale[j].x=player[i].x;
						sirpale[j].y=player[i].y;
						sirpale[j].z=0;
						sirpale[j].xv=random(1500)/1000.0f-0.75f;
						sirpale[j].yv=random(1500)/1000.0f-0.75f;
						sirpale[j].zv=random(1500)/1000.0f-0.75f;
						while(sqrt(sirpale[j].xv*sirpale[j].xv+sirpale[j].yv*sirpale[j].yv+sirpale[j].zv*sirpale[j].zv)>0.64f){
							sirpale[j].xv=random(1500)/1000.0f-0.75f;
							sirpale[j].yv=random(1500)/1000.0f-0.75f;
							sirpale[j].zv=random(1500)/1000.0f-0.75f;
						}
						sirpale[j].maxaika=400;
						sirpale[j].aika=random(200)+200;
						sirpale[j].koko=random(200)/100.0f+0.6f;
						sirpale[j].muoto=0;
						sirpale[j].red=random(50)/100.0f+0.5f;
						sirpale[j].green=random(100)/100.0f;
						sirpale[j].blue=random(100)/100.0f;
					}
					j++;
				}	

				player[i].suojapaalla=false;
				if(player[i].alus.easelaji==12){
					double tsaga=random(100);
					if(tsaga>=50){
						if(!joinpeli) player[i].uudestaan(alus[12]);
						if (efektitpaalla){
							if(alus[12].eosumisaani!=NULL) alus[12].eosumisaani->play();
						}
					}
				}
				if(player[i].energia<=0){
					if (efektitpaalla){
						if(player[i].laseraani) player[i].ampumisaani->stop();
						player[i].laseraani=false;
						if(alus[2].eampumisaani!=NULL) alus[2].eampumisaani->play();
					}
					player[i].kuolema=syntymisaika+kuolemisaika;
				}	

			}
		}
	}
	for(i=0;i<players;i++){
		if(player[i].pisteet>=0 || !pistepeli){

			if(player[i].kuolema==syntymisaika){
	//			player[i].kuolema=99;
				if(!joinpeli && !hostpeli && gamemode==CTF){
					int ctiimi=player[i].tiimi;
					int i2;
					for(i2=0;i2<players;i2++){
						if(player[i2].tiimi==ctiimi)player[i2].pisteet-=ckuolemapisteet;
					}
				}
				if(!joinpeli && gamemode==DOMINATION) {
					playeruudestaan[i]=1;
				}
				else if(!joinpeli && gamemode==MUSHROOM){
					playeruudestaan[i]=1;
					int ctiimi=player[i].tiimi;
					int i2;
					for(i2=0;i2<players;i2++){
						if(player[i2].tiimi==ctiimi)player[i2].pisteet-=skuolemapisteet;
					}
				}
				else if(!joinpeli && gamemode==CTF){
					playeruudestaan[i]=1;
					int ctiimi=player[i].tiimi;
					int i2;
					for(i2=0;i2<players;i2++){
						if(player[i2].tiimi==ctiimi)player[i2].pisteet-=ckuolemapisteet;
					}
				}else if(hostpeli && gamemode==DEATHMATCHRANDOM){
					if(player[i].pisteet>=alus[2].pistemaara) playeruudestaan[i]=1;
					else player[i].pisteet=-1;
				}
				else if(hostpeli && gamemode==DEATHMATCH) {
					if(player[i].pisteet>=alus[2].pistemaara){
						if(playeruudestaan[i]<2){
							playeruudestaan[i]=2;
						}
					}else player[i].pisteet=-1;
				}
				if(!joinpeli && !hostpeli){
					if(!pistepeli){
						double temp3=random(10000);
						int temp3int=(int)(temp3/10001.0f*(double)aluksia);
						if(temp3int>=aluksia)temp3int-=aluksia;
						if (temp3int<0) temp3int+=aluksia;
						player[i].uudestaan(alus[temp3int]);
					}else{//if pistepeli
						player[i].suojapaalla=false;
						if(player[i].pisteet<=alus[2].pistemaara){
							player[i].pisteet=-1;
						}else{
							if(saavalita)
								valitseAlus(i);
							else{
								int temp3=random(aluksia);
								while(player[i].pisteet<alus[temp3].pistemaara){
									temp3=random(aluksia);
								}
								player[i].uudestaan(alus[temp3]);
								player[i].pisteet-=alus[temp3].pistemaara;
							}
						}
					}
					player[i].laitaAlkuSirpaleet();
				}
			}
		}
	}
	if(!joinpeli) for(i=0;i<players;i++){
		if(player[i].pisteet>=0 || !pistepeli){
			if(player[i].kuolema==1){
				player[i].energia=player[i].alus.energia;
				player[i].xvauhti=0;
				player[i].yvauhti=0;
			}
		}
	}
	for(i=0;i<sirpaleita;i++) if(sirpale[i].aika>0){
		sirpale[i].x+=sirpale[i].xv;
		sirpale[i].y+=sirpale[i].yv;
		sirpale[i].z+=sirpale[i].zv;
		sirpale[i].aika--;
	}

	for(i=0;i<75;i++){
		double starxet=star[i].x+45.0f;
		double staryet=star[i].y-45.0f;
		double staret=starxet*starxet+staryet*staryet;
		star[i].z=sin((staret/70.0f)+(framecount/50.0f))*(-8.0f)-50.0f;
	}
	if(!joinpeli && gamemode==DOMINATION) laskeDPisteet();
	if(!joinpeli && gamemode==CTF) laskeCPisteet();
	if(!joinpeli && gamemode==MUSHROOM) laskeSPisteet();

}

void drawFrame(int framecount){
int i;	

	//resetetaan matriisi ja tyhjennet‰‰n bufferi
//	gluPerspective(45.0f,(GLfloat)screenwidth/(GLfloat)screenheight,camposz,camposz+alustaso-20);

//	gluPerspective(45.0f,(GLfloat)screenwidth/(GLfloat)screenheight,camposz-20,camposz-40);
	gluPerspective(45.0f,(GLfloat)screenwidth/(GLfloat)screenheight,camposz-20,camposz-40);
//	gluPerspective(45.0f,(GLfloat)screenwidth/(GLfloat)screenheight,0.1,40);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);			
	glLoadIdentity();							
//	glMatrixMode(GL_TEXTURE);
//	glScalef(1,-1,1);
	glMatrixMode(GL_MODELVIEW);

/*	gluLookAt(camposx,camposy,camposz,
	camposx,camposy,0,
	0,1,0);
*/
/*	gluLookAt(camposx/4,camposy/4,camposz,
	camposx/1,camposy/1,alustaso,
	0,1,0);
*/
	if(sets->cammode==0){
		gluLookAt(camposx/4,camposy/4,camposz,
				camposx/1,camposy/1,alustaso,
				cos(alfa2/180.0f*3.14f)*cos(beta/180.0f*3.14f),sin(alfa2/180.0f*3.14f)*cos(beta/180.0f*3.14f),-sin(beta/180.0f*3.14));
/*		gluLookAt(camposx/4,camposy/4,camposz,
				camposx/1,camposy/1,alustaso,
				cos(alfa/180.0f*3.14f),sin(alfa/180.0f*3.14f),0);*/
//				cos(alfa2/180.0f*3.14f),sin(alfa2/180.0f*3.14f),0);
	}else{
		gluLookAt(camposx,camposy,camposz,
				camposx,camposy,0,
				0,1,0);
	}
	
/*
	gluLookAt(0,0,-10,
	0,0,alustaso,
	0,1,0);
*/
//kolme valoa
//GLfloat lightposition[4] = {player[0].x, player[0].y, alustaso+20, 1};
GLfloat specular[4] = {1, 1, 1, 1};
glMaterialfv(GL_FRONT, GL_SPECULAR, specular);
GLfloat lightdiffuse[4]= {0.8, 0.8, 0.8, 0.8};
glLightfv(GL_LIGHT0,GL_DIFFUSE, lightdiffuse);
GLfloat lightposition[4] = {0, 0, -25, 1};
glLightfv(GL_LIGHT0,GL_POSITION,lightposition);

//GLfloat lightposition2[4] = {player[1].x, player[1].y, alustaso+20, 1};
//glLightfv(GL_LIGHT1,GL_POSITION,lightposition2);


//GLfloat lightposition3[4] = {0, -25, 10, 1};
//glLightfv(GL_LIGHT2,GL_POSITION,lightposition3);

//piirret‰‰n pelaajat&tilpehˆˆrit
//piirret‰‰n taustaflareja


//galaxi1
glBlendFunc(GL_ONE,GL_ONE);	
	double aukkosade=12;
	if(sets->detail>1){
		glColor4f(1.0f,1.0f,1.0f, 1.0f);		
		glBindTexture(GL_TEXTURE_2D, texture[23]);				
		glPushMatrix();
		glTranslatef(1.7*maxx,-1*maxy/2, alustaso-200);						
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*aukkosade, -1.0f*aukkosade,  0.0f);	
			glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*aukkosade, -1.0f*aukkosade,  0.0f);	
			glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*aukkosade,  1.0f*aukkosade,  0.0f);	
			glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*aukkosade,  1.0f*aukkosade,  0.0f);	
		glEnd();
		glPopMatrix();
	}

//galaxi2
	if(sets->detail>0){
		double aukkosade2=8;
		glColor4f(1.0f,1.0f,1.0f, 1.0f);		
		glBindTexture(GL_TEXTURE_2D, texture[26]);				
		glPushMatrix();
		glTranslatef(-0.7*maxx,-1*maxy/3, alustaso-200);						
		glBegin(GL_QUADS);
				glNormal3f(0,0,1.0f);
				glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*aukkosade2, -1.0f*aukkosade2,  0.0f);	
				glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*aukkosade2, -1.0f*aukkosade2,  0.0f);	
				glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*aukkosade2,  1.0f*aukkosade2,  0.0f);	
				glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*aukkosade2,  1.0f*aukkosade2,  0.0f);	
		glEnd();
		glPopMatrix();
	}

	if(sets->detail>0){
		glBindTexture(GL_TEXTURE_2D, texture[18]);				
		for(i=0;i<stars;i++){
			glPushMatrix();
				glTranslatef(star[i].x,star[i].y,alustaso+star[i].z);
				glColor4f(star[i].r,star[i].g,star[i].b, 1.0f);		
				glBegin(GL_QUADS);
					glNormal3f(0,0,1.0f);
					glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*star[i].koko, -1.0f*star[i].koko,  0.0f);	
					glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*star[i].koko, -1.0f*star[i].koko,  0.0f);	
					glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*star[i].koko,  1.0f*star[i].koko,  0.0f);	
					glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*star[i].koko,  1.0f*star[i].koko,  0.0f);	
				glEnd();
			glPopMatrix();
		}

	}

	for(i=0;i<players;i++){
		if(!pistepeli || player[i].pisteet>=0){
			if(player[i].kuolema<=0){
				player[i].piirra();
				player[i].piirraPadit();
				player[i].piirraePadit();
				player[i].piirraLimat();
				player[i].piirraKaasujaljet();
				if(player[i].alus.easelaji==5 ||player[i].alus.easelaji==6
					|| player[i].alus.easelaji==7 || player[i].alus.aselaji==9 ||player[i].alus.aselaji==11
					|| player[i].alus.numero==18){
//				if(joinpeli) printf("viivoja lasketaan\n");
					player[i].laskeViivat();
					player[i].piirraViivat();
				}
			}
		}
	}

	for(i=0;i<players;i++){
		if(!pistepeli || player[i].pisteet>=0){
			if(player[i].kuolema>0 && player[i].kuolema<syntymisaika){
				player[i].piirraAlkuSirpaleet();
			}
		}
	}
	piirraPikkumiehet();
	piirraAsteroidit();
	drawPlanet();

	//piirret‰‰n reunust‰hdet
	glPushMatrix();
	//glEnable(GL_BLEND);
	//glDisable(GL_LIGHTING);
	//glDisable(GL_DEPTH_TEST);
	//		glColor4f(1.0f,1.0f,1.0f, 0.5f);		
	glBlendFunc(GL_ONE,GL_ONE);	


	//if(x<0)alfa=180+alfa;
	//	printf("x=%f,y=%f,z=%f,xy=%f,alfa=%f,beta=%f\n",x,y,z,xy,alfa,beta);


	//	glBindTexture(GL_TEXTURE_2D, texture[3]);				
	glBindTexture(GL_TEXTURE_2D, texture[18]);				
	glTranslatef(maxx,maxy, alustaso);						
	int tahtikoko=6.0f;
	for(i=0;i<20;i++){
		glColor4f(1.0f*(double)((20.0f-i)/20.0f),0,1.0f*(double)(i/20.0f), 0.8f);		
		if(i!=0){
			glTranslatef(maxx/-10.0f, 0,0);
			glPushMatrix();
				if(sets->cammode==0){
					glRotatef(alfa,0,0,1);
					glRotatef(beta,0,1,0);
				}
				glBegin(GL_QUADS);
					glNormal3f(0,0,1.0f);	

					glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*tahtikoko, -1.0f*tahtikoko,  0.0f);	
					glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*tahtikoko, -1.0f*tahtikoko,  0.0f);	
					glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*tahtikoko,  1.0f*tahtikoko,  0.0f);	
					glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*tahtikoko,  1.0f*tahtikoko,  0.0f);	
				glEnd();
			glPopMatrix();
		}else{
			glEnable(GL_DEPTH_TEST);
			glEnable(GL_LIGHTING);
			glDisable(GL_BLEND);
			glDisable(GL_TEXTURE_2D);

			 glPushMatrix();
				glScalef(5,5,5);
				glRotatef(90,1,0,0);
				glCallList(KUKKA1);
			glPopMatrix();
			glEnable(GL_BLEND);
			glEnable(GL_TEXTURE_2D);
			glDisable(GL_LIGHTING);
			glDisable(GL_DEPTH_TEST);
		}
	}

	glTranslatef(maxx/-10.0f,0, 0);					

	for(i=0;i<20;i++){
		glColor4f(1.0f*(double)(i/20.0f),0,1.0f*(double)((20.0f-i)/20.0f), 0.8f);		
		if(i!=0){
			glTranslatef(0, maxy/-10.0f,0);
			glPushMatrix();
				if(sets->cammode==0){
					glRotatef(alfa,0,0,1);
					glRotatef(beta,0,1,0);
				}
				glBegin(GL_QUADS);
					glNormal3f(0,0,1.0f);

					glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*tahtikoko, -1.0f*tahtikoko,  0.0f);
					glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*tahtikoko, -1.0f*tahtikoko,  0.0f);
					glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*tahtikoko,  1.0f*tahtikoko,  0.0f);
					glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*tahtikoko,  1.0f*tahtikoko,  0.0f);
				glEnd();
			glPopMatrix();
		}else{
			glEnable(GL_DEPTH_TEST);
			glEnable(GL_LIGHTING);
			glDisable(GL_BLEND);
			glDisable(GL_TEXTURE_2D);

			 glPushMatrix();
				glScalef(5,5,5);
				glRotatef(90,1,0,0);
				glCallList(KUKKA2);
			glPopMatrix();
			glEnable(GL_BLEND);
			glEnable(GL_TEXTURE_2D);
			glDisable(GL_LIGHTING);
			glDisable(GL_DEPTH_TEST);
		}

	}

	glTranslatef(0, maxy/-10.0f,0);						

	for(i=0;i<20;i++){
		glColor4f(1.0f*(double)((20.0f-i)/20.0f),1.0f*(double)(i/20.0f),0, 0.8f);		
		if(i!=0){
			glTranslatef(maxx/+10.0f, 0,0);
			glPushMatrix();
				if(sets->cammode==0){
					glRotatef(alfa,0,0,1);
					glRotatef(beta,0,1,0);
				}
				glBegin(GL_QUADS);
					glNormal3f(0,0,1.0f);

					glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*tahtikoko, -1.0f*tahtikoko,  0.0f);
					glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*tahtikoko, -1.0f*tahtikoko,  0.0f);
					glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*tahtikoko,  1.0f*tahtikoko,  0.0f);
					glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*tahtikoko,  1.0f*tahtikoko,  0.0f);
				glEnd();
			glPopMatrix();
		}else{
			glEnable(GL_DEPTH_TEST);
			glEnable(GL_LIGHTING);
			glDisable(GL_BLEND);
			glDisable(GL_TEXTURE_2D);

			 glPushMatrix();
				glScalef(5,5,5);
				glRotatef(90,1,0,0);
				glCallList(KUKKA1);
			glPopMatrix();
			glEnable(GL_BLEND);
			glEnable(GL_TEXTURE_2D);
			glDisable(GL_LIGHTING);
			glDisable(GL_DEPTH_TEST);
		}

	}

	glTranslatef(maxx/+10.0f, 0,0);				
	for(i=0;i<20;i++){
		glColor4f(1.0f*(double)(i/20.0f),1.0f*(double)((20.0f-i)/20.0f),0, 0.8f);		
		if(i!=0){
			glTranslatef(0, maxy/+10.0f,0);
			glPushMatrix();
				if(sets->cammode==0){
					glRotatef(alfa,0,0,1);
					glRotatef(beta,0,1,0);
				}
				glBegin(GL_QUADS);
					glNormal3f(0,0,1.0f);

					glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*tahtikoko, -1.0f*tahtikoko,  0.0f);
					glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*tahtikoko, -1.0f*tahtikoko,  0.0f);
					glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*tahtikoko,  1.0f*tahtikoko,  0.0f);
					glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*tahtikoko,  1.0f*tahtikoko,  0.0f);
				glEnd();
			glPopMatrix();
		}else{
			glEnable(GL_DEPTH_TEST);
			glEnable(GL_LIGHTING);
			glDisable(GL_BLEND);
			glDisable(GL_TEXTURE_2D);

			 glPushMatrix();
				glScalef(5,5,5);
				glRotatef(90,1,0,0);
				glCallList(KUKKA3);
			glPopMatrix();
			glEnable(GL_BLEND);
			glEnable(GL_TEXTURE_2D);
			glDisable(GL_LIGHTING);
			glDisable(GL_DEPTH_TEST);
		}
	}

	glBlendFunc(GL_SRC_ALPHA,GL_ONE_MINUS_SRC_COLOR);
//glEnable(GL_LIGHTING);

glPopMatrix();


	if(gamemode==MUSHROOM){
		piirraSPisteet();
	}
	else if(gamemode==DOMINATION){
		piirraDPisteet();
	}
	else if(gamemode==CTF){
		piirraCPisteet();
	}


//piirret‰‰n sirpaleet
glEnable(GL_BLEND);
glDisable(GL_DEPTH_TEST);
glEnable(GL_TEXTURE_2D);
glDisable(GL_LIGHTING);
glBlendFunc(GL_ONE,GL_ONE);	
	for(i=0;i<sirpaleita;i++) if(sirpale[i].aika>0){
		//if(joinpeli && i<10) printf("piirret‰‰n sirpale %i,%i\n",sirpale[i].x,sirpale[i].y);
		if(sirpale[i].muoto == 10)
			glBindTexture(GL_TEXTURE_2D, texture[15]);		
		else glBindTexture(GL_TEXTURE_2D, texture[18]);		
		glColor4f(sirpale[i].red*(double)sirpale[i].aika/sirpale[i].maxaika,sirpale[i].green*(double)sirpale[i].aika/sirpale[i].maxaika,sirpale[i].blue*(double)sirpale[i].aika/sirpale[i].maxaika, 1.0f);		
		glPushMatrix();
		glTranslatef(sirpale[i].x,sirpale[i].y,alustaso+sirpale[i].z);
		if(sets->cammode==0){
			glRotatef(alfa,0,0,1);
			glRotatef(beta,0,1,0);
		}
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*sirpale[i].koko, -1.0f*sirpale[i].koko, 0);
			glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*sirpale[i].koko, -1.0f*sirpale[i].koko, 0);
			glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*sirpale[i].koko,  1.0f*sirpale[i].koko, 0);
			glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*sirpale[i].koko,  1.0f*sirpale[i].koko, 0);
		glEnd();
		glPopMatrix();
	}



/*	for(i=0;i<sirpaleita;i++) if(sirpale[i].aika>0){
		glPushMatrix();
		glTranslatef(sirpale[i].x,sirpale[i].y,alustaso);
		glBegin(GL_QUADS);		
			glColor4f(sirpale[i].red,sirpale[i].green,sirpale[i].blue, (double)sirpale[i].aika/sirpale[i].maxaika);		
			glVertex3f(-1.0f*sirpale[i].koko, -1.0f*sirpale[i].koko, 0);
			glVertex3f( 1.0f*sirpale[i].koko, -1.0f*sirpale[i].koko, 0);
			glVertex3f( 1.0f*sirpale[i].koko,  1.0f*sirpale[i].koko, 0);
			glVertex3f(-1.0f*sirpale[i].koko,  1.0f*sirpale[i].koko, 0);
		glEnd();
		glPopMatrix();

	}
*/

	glBlendFunc(GL_SRC_COLOR,GL_ONE_MINUS_SRC_COLOR);

	glEnable(GL_BLEND);
	glEnable(GL_TEXTURE_2D);
	glDisable(GL_LIGHTING);
	glDisable(GL_DEPTH_TEST);

//piirret‰‰n tilastot
//	glEnable(GL_TEXTURE_2D);
/*	glPopMatrix();
	glPushMatrix();*/
	glLoadIdentity();	
/*	gluLookAt(0,1,0,
			 0,0,0,
			 0,1,0);*/
	glTranslatef(0,0,-30);
	for(i=0;i<players;i++){
		player[i].piirraGraffat();
	}

	if(gamemode==DOMINATION){
		piirraDGraffat();
	}
	else if(gamemode==CTF){
		piirraCGraffat();
	}else if(gamemode==MUSHROOM){
		piirraSGraffat();
	}

	glPopMatrix();
//	glEnable(GL_TEXTURE_2D);

//pyˆritet‰‰n kuutiota
	xrot+=0.6f;								// X Axis Rotation
	yrot+=0.9f;								// Y Axis Rotation
	zrot+=0.12f;								// Z Axis Rotation

	SDL_GL_SwapBuffers();
}

void drawPlanet(){
	/*
	for(i=0;i<=n;i++) {
		scalar h = -1 + 2*(scalar)(i-1)/(scalar)(n-1);
		theta[i] = acos(h);
		if (i==1 || i==n) phi[i] = 0;
		else phi[i] = fmod(phi[i-1] + 3.6/sqrt(n*(1-h*h)),2*PI);
		Vector direction(cos(theta[i])*sin(phi[i]),sin(theta[i])*sin(phi[i]),cos(phi[i]));
		Vector energy(light->illumination(light->trans.transformDirectionVector(direction))*e);
		tracePhoton(raytracer, energy, stack, 0, light->trans.getOrigin(), direction);
	}
	for k=1 to N do
		h = -1 + 2*(k-1)/(N-1)
		theta[k] = arccos(h)
		if k=1 or k=N then phi[k] = 0
		else phi[k] = (phi[k-1] + 3.6/sqrt(N*(1-h^2))) mod (2*pi)
	endfor
	
	>N:=args[1]:
	>
	>beta:=4/sqrt(N);
	>
	>if nargs<2 then n:=floor(sqrt(Pi*N)/2)+1;
	>else n:=args[2]; fi;
	>
	>theta:=(Pi-beta)/(n-2);
	>
	>mbar[1]:=1; mbar[n]:=1;
	>
	>for i from 2 to n-1 do
	>  mbar[i]:=evalf(N*(cos(theta*(i-2)+beta/2)-cos(theta*(i-1)+beta/2))/2);
	>od;
	>
	>alpha:=0;
	>for i from 1 to n do
	>  if (mbar[i]-floor(mbar[i]+alpha)) < 0.5 then
	>   m[i]:=floor(mbar[i]+alpha);
	>  else
	>   m[i]:=floor(mbar[i]+alpha) + 1;fi;
	>  alpha:=alpha + mbar[i]-m[i];
	>od;
	>
	>printf("For N=%d, the number of levels cuts n=%d\n", N, n);
	>printf("--------------------------------------------------\n");
	>for i from 1 to n do
	>  printf("mbar[%2d} =%9.9f   m[%2d] =%4d   
	>diff=%9.9f\n",i,mbar[i],i,m[i],mbar[i]-m[i]);
	>od;
	>
	>L:=[[0,0,1]];
	>for i from 2 to n-1 do
	>  t:=(i-2+1/2)*theta+beta/2;
	>  r:=sin(t);
	>  h:=cos(t);
	>  alpha:=((i-2) mod 2)/2;
	>
	>  for j from 1 to m[i] do
	>   s:=2*Pi*(j-1+alpha)/m[i];
	>   L:=[op(L),[evalf(r*cos(s),20),evalf(r*sin(s),20),evalf(h,20)]];
	>  od;
	>od;
	*/
	glEnable(GL_LIGHTING);
	glEnable(GL_DEPTH_TEST);
	glDisable(GL_BLEND);
	glDisable(GL_TEXTURE_2D);
	glEnable(GL_CULL_FACE);
	glPushMatrix();

	glTranslatef(0, 0, alustaso);

	glScalef(2.2,2.2,2.2);

	glRotatef(xrot,1.0f,0.0f,0.0f);	
	glRotatef(yrot,0.0f,1.0f,0.0f);	
	glRotatef(zrot,0.0f,0.0f,1.0f);

	glColor3f(1, 1, 1);
	GLUquadricObj *gah=gluNewQuadric();
	gluSphere(gah, 3, 8+sets->detail*4, 8+sets->detail*4);
	gluDeleteQuadric(gah);

#define N 60
#define n 8
	//n = sqrt(PI*N)/2+1;
	double beta = 4.0/sqrt(N);
	double theta = (PI-beta)/(n-2);
	double mbar[n];// = new double[n];
	int m[n];// = new int[n];
	mbar[0] = 1;
	mbar[n-1] = 1;

	int i, j;
	for (i = 1; i < n-1; i++){
	  mbar[i] = N*(cos(theta*(i-1)+beta/2) - cos(theta*i+beta/2))/2;
	}
	double alpha = 0;
	for (i = 0; i < n; i++){
		if (mbar[i] - (int)(mbar[i]+alpha) < 0.5){
			m[i] = (mbar[i]+alpha);
		} else{
			m[i] = (mbar[i]+alpha) + 1;
		}
		alpha += mbar[i]-m[i];
	}
	//printf("n: %i\n", n);
	for (i = 0; i < n; i++){
		//printf("m[%i]: %i\n", i, m[i]);
		double t = (i-1 + 0.5)*theta + beta/2;
//		double r = sin(t);
//		double h = cos(t);
		alpha = ((i-1) & 1)/2.0;
		GLUquadricObj *gah=gluNewQuadric();
		for (j = 0; j < m[i]; j++){
			double s = 2*PI*(j+alpha)/m[i];
			glPushMatrix();

			glRotatef(s*360/(2*PI), 0, 1, 0);
			glRotatef(t*360/(2*PI)+90, 1, 0, 0);

			glTranslatef(0, 0, 2.8);
			gluCylinder(gah, 0.5, 0, 1, 8+sets->detail*4, 1+sets->detail*2);
			glPopMatrix();
		}
		gluDeleteQuadric(gah);
	}
	/*int k;
	double phi;
	for (k = 0; k < n; k++){
		glPushMatrix();

		double h = -1 + 2*(double)k/(n-1);
		double theta = acos(h);
		if (k == 0 || k == n-1) phi = 0;
		else phi = phi + 3.6/sqrt(n*(1-h*h));
		if (phi > 2*PI) phi-=2*PI;
		
		glRotatef(theta*360/(2*PI), 0, 1, 0);
		glRotatef(phi*360/(2*PI), 1, 0, 0);

		glTranslatef(0, 0, 3);
		gluCylinder(gluNewQuadric(), 0.5, 0, 1, 10, 3);
		glPopMatrix();
	}*/

	glPopMatrix();
	glDisable(GL_CULL_FACE);
	glEnable(GL_TEXTURE_2D);
	glEnable(GL_BLEND);
	glDisable(GL_LIGHTING);
}


void valitseAlus(int numero){
	player[numero].pfire=false;
	player[numero].palas=false;
	player[numero].pylos=false;
	int randomluku=random(aluksia);
	int vastustaja;
	for(vastustaja=0;vastustaja<players;vastustaja++){
		if(player[vastustaja].tiimi!=player[numero].tiimi) break;
	}
	if(player[numero].ihmispelaaja==false){
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
		if(player[numero].pisteet<50){
			if(randomluku<0 || randomluku>=aluksia)randomluku=random(aluksia);
			while(alus[randomluku].pistemaara>player[numero].pisteet){
				randomluku=random(aluksia);
			}
		}
	}

	gluPerspective(45.0f,(GLfloat)screenwidth/(GLfloat)screenheight,1.0f,100.0f);
	bool menuready=false;
	bool valintaready=false;
	int selection=0;
	currenttime=SDL_GetTicks()-starttime;
	int frametemp=calculatefps*currenttime/1000.0;
	while(!valintaready){
		while(!menuready){
			processEvents();
			currenttime=SDL_GetTicks()-starttime;
			if(player[numero].ihmispelaaja==false){
				entterpressed=false;
				player[numero].pylos=false;
				player[numero].palas=false;
				player[numero].pfire=false;
				player[numero].pright=false;
				player[numero].pleft=false;
			}
/*			if(ai && numero && randomluku==0) while(frametemp+50>calculatefps*currenttime/1000.0){
				frametemp=calculatefps*currenttime/1000.0;
			}*/
			if(player[numero].ihmispelaaja==false && players>3){
				if(randomluku!=0){
					if(randomluku>0){
						randomluku--;
						player[numero].palas=true;
					}else {
						randomluku++;
						player[numero].pylos=true;
					}
				}else{
					player[numero].pfire=true;
				}
			}
			else if(player[numero].ihmispelaaja==false && frametemp+5<calculatefps*currenttime/1000.0){
				if(randomluku!=0){
					frametemp=calculatefps*currenttime/1000.0;
					if(randomluku>0){
						randomluku--;
						player[numero].palas=true;
					}
					else{
						randomluku++;
						player[numero].pylos=true;
					}
				}else if(frametemp+50<calculatefps*currenttime/1000.0){
					frametemp=calculatefps*currenttime/1000.0;
					player[numero].pfire=true;
				}
			}
			if((entterpressed || player[numero].pfire)){
				player[numero].pfire=false;
				entterpressed=false;
				menuready=true;
			}else if((player[numero].pylos || player[numero].pright)){
				if(selection>0) selection--;
				else if(selection==0)selection=aluksia;
				player[numero].pylos=false;
				player[numero].pright=false;
			}else if((player[numero].palas || player[numero].pleft)){
				if(selection<=aluksia-1) selection++;
				else if(selection==aluksia) selection=0;
				player[numero].palas=false;
				player[numero].pleft=false;
			}
			if(player[numero].ihmispelaaja || players<4)
				do{
					currenttime=SDL_GetTicks()-starttime;
					framecounter=calculatefps*currenttime/1000.0;
				}	while (oldframecounter==framecounter);

			oldframecounter=framecounter;
			if(player[numero].ihmispelaaja || players<4)
				drawAlusValinta(numero, selection);
			updateAudio();
			if(player[numero].ihmispelaaja==false){
				player[numero].pfire=false;
				player[numero].palas=false;
				player[numero].pylos=false;
			}
		}
		//random-alus
		if(selection==aluksia){
			int randomluku2=random(aluksia);
			while(alus[randomluku2].pistemaara>player[numero].pisteet){
				randomluku2=random(aluksia);
			}
			player[numero].uudestaan(alus[randomluku2]);
			player[numero].pisteet-=alus[randomluku2].pistemaara;
			valintaready=true;
		}
		//v‰hemm‰n random
		else{
			if(player[numero].pisteet>=alus[selection].pistemaara){
				player[numero].uudestaan(alus[selection]);
				player[numero].pisteet-=alus[selection].pistemaara;
				valintaready=true;
			}else {
				selection=2;
				menuready=false;
			}
		}
		escpressed=false;
	}

}

void drawAlusValinta(int numero, int alusnumero){
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

		//ship number & Name
		gl_println(-25, 3.5,-40,vekotin[7]);
		gl_println(-10, 3.5,-40,vekotin[8]);
		gl_println(-5, 3.5,-40,alus[alusnumero].nimi);

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


//	glEnable(GL_LIGHTING);
//	glDisable(GL_TEXTURE_2D);
	SDL_GL_SwapBuffers();
}


void laskePikkumiehet(){
	int i,j;
	for(i=0;i<pikkumiehia;i++){
		if(pikkumies[i].aika>0){
			pikkumies[i].aika--;
			if(pikkumies[i].ampumisviive>0){
				pikkumies[i].ampumisviive--;
			}else{
				for(j=0;j<players;j++){
					if(!pistepeli || player[j].pisteet>=0){
						double xet=pikkumies[i].x-player[j].x;
						double yet=pikkumies[i].y-player[j].y;
						double etaisyys=sqrt(xet*xet+yet*yet);
						if(etaisyys<player[j].alus.sade){
							player[j].energia++;
							pikkumies[i].aika=0;
						}
					}
				}
			}
			if(pikkumies[i].aika<=0){
				if(hostpeli){
					int i2;
					for(i2=0;i2<10;i2++){
						if(siirrot[i2].aika<=0){
							siirrot[i2].x=pikkumies[i].x;
							siirrot[i2].y=pikkumies[i].y;
							siirrot[i2].aika=pikkumies[i].aika;
							siirrot[i2].energia=pikkumies[i].energia;
							siirrot[i2].numero=i;
							i2=10;
						}
					}
				}
			}
		}
	}
}

void laskeAsteroidit(){
	int i,j,k;

	if(joinpeli && asteroidirajahtaa){
		asteroidirajahtaa=false;
		//sirpaleet
		int scount=35+sets->detail*15;
		int j2=0;
		while(j2<sirpaleita && scount>0){
			if(sirpale[j2].aika<=0){
				scount--;
				sirpale[j2].x=arajahdys.x;
				sirpale[j2].y=arajahdys.y;
				sirpale[j2].z=0;
				sirpale[j2].xv=random(200)/500.0f-0.2f;
				sirpale[j2].yv=random(200)/500.0f-0.2f;
				sirpale[j2].zv=random(200)/500.0f-0.2f;
				while(sirpale[j2].xv*sirpale[j2].xv+sirpale[j2].yv+sirpale[j2].yv
					  +sirpale[j2].zv*sirpale[j2].zv>0.09f){
					sirpale[j2].xv=random(200)/500.0f-0.2f;
					sirpale[j2].yv=random(200)/500.0f-0.2f;
					sirpale[j2].zv=random(200)/500.0f-0.2f;
				}
				sirpale[j2].maxaika=250;
				sirpale[j2].aika=random(200)+50;
				sirpale[j2].koko=random(30)/100.0f+0.2f;
				sirpale[j2].muoto=0;
				sirpale[j2].red=random(20)/100.0f+0.5f;
				sirpale[j2].green=random(20)/100.0f+0.5f;
				sirpale[j2].blue=random(15)/100.0f+0.35f;
			}
			j2++;
		}
	}


	for(i=0;i<asteroideja;i++){
		double akoko=asteroidi[i].koko;
		//asteroidi vs planeetta
		double aetaisyys=asteroidi[i].x*asteroidi[i].x+asteroidi[i].y*asteroidi[i].y;
		if(aetaisyys<pow(planeettasade+1,2)){
			if(!joinpeli)asteroidi[i].energia=0;
		}
		//asteroidi vs asteroidi
		for(j=i+1;j<asteroideja;j++) {
			double axet=asteroidi[j].x-(asteroidi[i].x+asteroidi[i].xv);
			double ayet=asteroidi[j].y-(asteroidi[i].y+asteroidi[i].yv);
			if(axet*axet+ayet*ayet<pow((asteroidi[j].koko+akoko)/4*PI,2)){
				double kulma5=atan2(ayet, axet);
				double xtemp=asteroidi[j].xv;
				double ytemp=asteroidi[j].yv;
				double tormays=sqrt((xtemp-asteroidi[i].xv)*(xtemp-asteroidi[i].xv)+(ytemp-asteroidi[i].yv)*(ytemp-asteroidi[i].yv));
				tormays=tormays*(1/(1/asteroidi[j].koko+1/akoko))*2;
				asteroidi[j].xv+= cos(kulma5)*tormays/ asteroidi[j].koko;
				asteroidi[j].yv+= sin(kulma5)*tormays/ asteroidi[j].koko;
				asteroidi[i].xv-= cos(kulma5)*tormays / (akoko);
				asteroidi[i].yv-= sin(kulma5)*tormays / (akoko);
			}
		}
		//asteroidi vs players
		for(j=0;j<players;j++){
			if(!pistepeli || player[j].pisteet>=0){
				double axet=player[j].x-(asteroidi[i].x+asteroidi[i].xv);
				double ayet=player[j].y-(asteroidi[i].y+asteroidi[i].yv);
				if(axet*axet+ayet*ayet<pow((player[j].alus.sade+akoko)/4*PI,2)){
					double kulma5=atan2(ayet, axet);
					double xtemp=player[j].xvauhti;
					double ytemp=player[j].yvauhti;
					double tormays=sqrt((xtemp-asteroidi[i].xv)*(xtemp-asteroidi[i].xv)+(ytemp-asteroidi[i].yv)*(ytemp-asteroidi[i].yv));
					tormays=tormays*(1/(1/player[j].alus.paino+1/(akoko*100.0f)))*2;
					if(player[j].alus.aselaji==6 || player[j].alus.aselaji==18 || player[j].alus.numero==22){
						player[j].x-=(asteroidi[i].x-player[j].x)*0.4f;
						player[j].y-=(asteroidi[i].y-player[j].y)*0.4f;
					}else{
						player[j].xvauhti+=cos(kulma5)*tormays/ player[j].alus.paino;
						player[j].yvauhti+=sin(kulma5)*tormays/ player[j].alus.paino;
					}
					asteroidi[i].xv-= cos(kulma5)*tormays / (akoko*100.0f);
					asteroidi[i].yv-= sin(kulma5)*tormays / (akoko*100.0f);
					if(!joinpeli) if(player[j].alus.easelaji==14 || player[j].alus.easelaji==15){
						if(player[j].alus.easelaji==14){
							asteroidi[i].energia-=player[j].alus.edamage*player[j].fuel/player[j].alus.fuel;
							player[j].fuel=0;
						}
						else asteroidi[i].energia-=player[j].alus.edamage;
					}
				}
			
				//asteroidi vs playereitten padit
				for(k=0;k<player[j].alus.maxpadit;k++) if(player[j].pati[k].aika>0){
					double apxet=player[j].pati[k].x-(asteroidi[i].x+asteroidi[i].xv);
					double apyet=player[j].pati[k].y-(asteroidi[i].y+asteroidi[i].yv);
//					double apet=sqrt(apxet*apxet+apyet*apyet);
//					if(apet<(akoko+player[j].alus.patisade)/4.0f*PI){
					double apet=apxet*apxet+apyet*apyet;
					if(apet<pow((akoko+player[j].alus.patisade)/4.0f*PI,2)){
						if(!joinpeli) asteroidi[i].energia-=player[j].alus.damage;
						if(!joinpeli) player[j].pati[k].aika=0;
					}
				}
				//asteroidi vs playereitten epadit
				if(player[j].alus.numero!=9)
				for(k=0;k<player[j].alus.maxepadit;k++) 
				if(player[j].epati[k].aika>0){
					double apxet=player[j].epati[k].x-(asteroidi[i].x+asteroidi[i].xv);
					double apyet=player[j].epati[k].y-(asteroidi[i].y+asteroidi[i].yv);
//					double apet=sqrt(apxet*apxet+apyet*apyet);
//					if(apet<(akoko+player[j].alus.epatisade)/4.0f*PI){
					double apet=apxet*apxet+apyet*apyet;
					if(apet<pow((akoko+player[j].alus.epatisade)/4.0f*PI,2)){
						if(!joinpeli)asteroidi[i].energia-=player[j].alus.edamage;
						if(!joinpeli) player[j].epati[k].aika=0;
					}
				}
			}
		}
		//asteroidin liikkuminen
		asteroidi[i].xv-=asteroidi[i].x/aetaisyys*0.1;
		asteroidi[i].yv-=asteroidi[i].y/aetaisyys*0.1;
		if(sqrt(asteroidi[i].xv*asteroidi[i].xv+asteroidi[i].yv*asteroidi[i].yv)>0.7){
			asteroidi[i].xv/=1.01;
			asteroidi[i].yv/=1.01;
		}
		asteroidi[i].x+=asteroidi[i].xv;
		asteroidi[i].y+=asteroidi[i].yv;
	
		//asteroidin kuolema
		if(asteroidi[i].energia<=0 && !joinpeli){

			//sirpaleet
			int scount=35+sets->detail*15;
			int j2=0;
			while(j2<sirpaleita && scount>0){
				if(sirpale[j2].aika<=0){
					scount--;
					sirpale[j2].x=asteroidi[i].x;
					sirpale[j2].y=asteroidi[i].y;
					sirpale[j2].z=0;
					sirpale[j2].xv=random(200)/500.0f-0.2f;
					sirpale[j2].yv=random(200)/500.0f-0.2f;
					sirpale[j2].zv=random(200)/500.0f-0.2f;
					while(sirpale[j2].xv*sirpale[j2].xv+sirpale[j2].yv+sirpale[j2].yv
						+sirpale[j2].zv*sirpale[j2].zv>0.09f){
						sirpale[j2].xv=random(200)/500.0f-0.2f;
						sirpale[j2].yv=random(200)/500.0f-0.2f;
						sirpale[j2].zv=random(200)/500.0f-0.2f;
					}
					sirpale[j2].maxaika=250;
					sirpale[j2].aika=random(200)+50;
					sirpale[j2].koko=random(30)/100.0f+0.2f;
					sirpale[j2].muoto=0;
					sirpale[j2].red=random(20)/100.0f+0.5f;
					sirpale[j2].green=random(20)/100.0f+0.5f;
					sirpale[j2].blue=random(15)/100.0f+0.35f;
				}
				j2++;
			}

			//uusi asteroidi
				asteroidiuudestaan[i]=true;
				asteroidirajahtaa=true;
				arajahdys.x=asteroidi[i].x;
				arajahdys.y=asteroidi[i].y;
				asteroidi[i].koko=random(50)/100.0f+0.8f;
				asteroidi[i].alku=random(10000)/10.0f;
				asteroidi[i].r=random(300)/100.0f-1.5f;
				asteroidi[i].g=random(300)/100.0f-1.5f;
				asteroidi[i].b=random(300)/100.0f-1.5f;
				asteroidi[i].energia=asteroidienergia;
				double raja=maxx*1.0;
				double raja2=maxx*1.2;
				asteroidi[i].x=raja-1;
				asteroidi[i].y=raja-1;
				while(fabs(asteroidi[i].x)<raja && fabs(asteroidi[i].y)<raja){
					asteroidi[i].x=random(raja2*2)-raja2;
					asteroidi[i].y=random(raja2*2)-raja2;
				}
				asteroidi[i].xv=asteroidi[i].x;
				asteroidi[i].yv=asteroidi[i].y;
				while(fabs(atan2(asteroidi[i].yv,asteroidi[i].xv)
					  -atan2(asteroidi[i].y,asteroidi[i].x))
					  <PI/6){
					asteroidi[i].xv=random(1000)/2000.0f-0.25f;
					asteroidi[i].yv=random(1000)/2000.0f-0.25f;
				}
		}

	}
}

void piirraAsteroidit(){
	int i;
	glEnable(GL_LIGHTING);
	glEnable(GL_TEXTURE_2D);
	glDisable(GL_BLEND);
	glEnable(GL_DEPTH_TEST);			
	glBindTexture(GL_TEXTURE_2D, texture[39]);
	glColor4f(1,1,1,1.0f);		
//	GLUquadricObj *gah=gluNewQuadric();
//	gluQuadricTexture(gah,true);
//	gluQuadricDrawStyle(gah, GLU_FILL); 
//	gluQuadricNormals(gah,GLU_SMOOTH); 
	for(i=0;i<asteroideja;i++){
		glPushMatrix();
		glTranslatef(asteroidi[i].x,asteroidi[i].y,alustaso);
		glRotatef(xrot*asteroidi[i].r+asteroidi[i].alku,1.0f,0.0f,0.0f);	
		glRotatef(yrot*asteroidi[i].g+asteroidi[i].alku,0.0f,1.0f,0.0f);	
		glRotatef(zrot*asteroidi[i].b+asteroidi[i].alku,0.0f,0.0f,1.0f);
//		double koko=asteroidi[i].koko;
//		gluSphere(gah, koko, 12, 8);
		glScalef(asteroidi[i].koko,asteroidi[i].koko,asteroidi[i].koko);
		glCallList(list_asteroid);
	 glPopMatrix();
	}
//	gluDeleteQuadric(gah);
	//glEnable(GL_TEXTURE_2D);
	glEnable(GL_BLEND);
	glDisable(GL_DEPTH_TEST);			
	glDisable(GL_LIGHTING);
}

void piirraPikkumiehet(){
	int i;
	glEnable(GL_BLEND);
	glBlendFunc(GL_ONE,GL_ONE);	
	for(i=0;i<pikkumiehia;i++) if(pikkumies[i].aika>0){
		glPushMatrix();
		glTranslatef(pikkumies[i].x,pikkumies[i].y,alustaso);
//		glRotatef(pikkuaija[i].kulma-90.0f,0.0f,0.0f,1.0f);						
		glRotatef(-90.0f,0.0f,0.0f,1.0f);						
		glBindTexture(GL_TEXTURE_2D, texture[16]);
		glColor4f(1.0f,1.0f,1.0f,1.0f);		
		double koko=0.4;
		glBegin(GL_QUADS);
			glNormal3f(0,0,1.0f);
			glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*koko, -1.0f*koko,  1.0f);	// Bottom Left Of The Texture and Quad
			glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*koko, -1.0f*koko,  1.0f);	// Bottom Right Of The Texture and Quad
			glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*koko,  1.0f*koko,  1.0f);	// Top Right Of The Texture and Quad
			glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*koko,  1.0f*koko,  1.0f);	// Top Left Of The Texture and Quad
		glEnd();
		glPopMatrix();
	}

}
float alfa=0,beta=0,alfa2=0,beta2=0;
