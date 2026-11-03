#include "main.h"
#include "net.h"
#include "ctf.h"
#include "sieni.h"
#include "nethelp.h"
#include "Player.h"

//int coordkoko=17;
int coordkoko=13;
int kirjotaCoords(char *msg, Player *player){
	signed short tempkulma=player->kulma*150.0f;
	//signed short tempenergia=player->energia*150.0f;
	//signed short tempfuel=player->fuel*150.0f;
	unsigned char tempfuel=(float)player->fuel/(float)player->alus.fuel*200.0f;
	unsigned char tempenergia=(float)player->energia/(float)player->alus.energia*200.0f;
	signed short tmppisteet=(short)player->pisteet;
	//printf("vauhti:%f,%f\n",player->xvauhti,player->yvauhti);

	signed short tempx=player->x*150.0f;
	signed short tempy=player->y*150.0f;

	if(player->x*150.0f>32760) tempx=32760;
	else if(player->x*150.0f<-32760) tempx=-32760;
	if(player->y*150.0f>32760) tempy=32760;
	else if(player->y*150.0f<-32760) tempy=-32760;

	if(player->fuel>1.25f*(float)player->alus.fuel) tempfuel=200.0f*1.25f;
	if(player->energia>1.25f*(float)player->alus.energia) tempenergia=200.0f*1.25f;
	if(player->energia<0) tempenergia=0;
	if(player->fuel<0) tempfuel=0;

	if(player->kuolema>0) tempenergia=0;

	int n=0;

	memcpy(&msg[n],&tempx,sizeof(short));
	n+=sizeof(short);
	memcpy(&msg[n],&tempy,sizeof(short));
	n+=sizeof(short);
		
	/*memcpy(&msg[n],&player->x,sizeof(float));
	n+=sizeof(float);
	memcpy(&msg[n],&player->y,sizeof(float));
	n+=sizeof(float);*/

	memcpy(&msg[n],&tempkulma,sizeof(short));
	n+=sizeof(short);
	/*memcpy(&msg[n],&tempenergia,sizeof(short));
	n+=sizeof(short);
	memcpy(&msg[n],&tempfuel,sizeof(short));
	n+=sizeof(short);*/
	
	memcpy(&msg[n],&tempenergia,sizeof(char));
	n+=sizeof(char);
	memcpy(&msg[n],&tempfuel,sizeof(char));
	n+=sizeof(char);

	memcpy(&msg[n],&player->kuolema,sizeof(short));
	n+=sizeof(short);

	memcpy(&msg[n],&tmppisteet,sizeof(short));
	n+=sizeof(short);

	unsigned char napit=0;
	if(player->pfire) napit+=1;
	if(player->pease) napit+=2;
	if(player->pylos) napit+=4;
	if(player->pleft) napit+=8;
	if(player->pright) napit+=16;
	if(player->suojapaalla) napit+=32;
	memcpy(&msg[n],&napit,sizeof(char));
	n+=sizeof(char);
	//n=19;

	return n;
}

int lueCoords(Player *player, char *msg){

	unsigned char tempenergia;
	unsigned char tempfuel;
	signed short tempkulma;
//	signed short tempenergia;
//	signed short tempfuel;
	signed short tmppisteet;
	signed short tempx;
	signed short tempy;

	int n=0;

	/*memcpy(&player->x,&msg[n],sizeof(float));
	n+=sizeof(float);
	memcpy(&player->y,&msg[n],sizeof(float));
	n+=sizeof(float);*/

	memcpy(&tempx,&msg[n],sizeof(short));
	n+=sizeof(short);
	memcpy(&tempy,&msg[n],sizeof(short));
	n+=sizeof(short);

	memcpy(&tempkulma,&msg[n],sizeof(short));
	n+=sizeof(short);
/*	memcpy(&tempenergia,&msg[n],sizeof(short));
	n+=sizeof(short);
	memcpy(&tempfuel,&msg[n],sizeof(short));
	n+=sizeof(short);*/
	
	memcpy(&tempenergia,&msg[n],sizeof(char));
	n+=sizeof(char);
	memcpy(&tempfuel,&msg[n],sizeof(char));
	n+=sizeof(char);

	if(player->kuolema!=1) memcpy(&player->kuolema,&msg[n],sizeof(short));
	n+=sizeof(short);

	memcpy(&tmppisteet,&msg[n],sizeof(short));
	n+=sizeof(short);

	unsigned char napit=msg[n];
	n+=sizeof(char);

	player->joinase=false;
	player->joinease=false;
	player->kaasu=false;
	player->joinleft=false;
	player->joinright=false;
	player->suojapaalla=false;

	if(napit&1) player->joinase=true;
	if(napit&2) player->joinease=true;
	if(napit&4) player->kaasu=true;
	if(napit&8) player->joinleft=true;
	if(napit&16) player->joinright=true;
	if(napit&32) player->suojapaalla=true;

//			if(player[i].joinase)printf("p%i jase=1\n",i);
//			else printf("p%i jase=0\n",i);
			//n+=sizeof(coords);

	player->pisteet=(float)tmppisteet;
	player->kulma=(float)tempkulma/150.0f;
/*	player->energia=(float)tempenergia/150.0f;
	player->fuel=(float)tempfuel/150.0f;*/

	player->energia=(float)tempenergia/200.0f*(float)player->alus.energia;
	player->fuel=(float)tempfuel/200.0f*(float)player->alus.fuel;

	player->xvauhti=(float)tempx/150.0f-player->x;
	player->yvauhti=(float)tempy/150.0f-player->y;
	player->x=(float)tempx/150.0f;
	player->y=(float)tempy/150.0f;

//	printf("p=%f,k=%f,e=%f,f=%f\n",player->pisteet,player->kulma,player->energia,player->fuel);

	//n=19;
	return n;
}

int pikkumieskoko=16;
int kirjotaPikkumiessiirto(char *msg, pikkumiessiirto *siirto){
	int n=0;

	memcpy(&msg[n],&siirto->x,sizeof(float));
	n+=4;
	memcpy(&msg[n],&siirto->y,sizeof(float));
	n+=4;
	memcpy(&msg[n],&siirto->energia,sizeof(float));
	n+=4;

	memcpy(&msg[n],&siirto->aika,sizeof(short));
	n+=2;
	memcpy(&msg[n],&siirto->numero,sizeof(short));
	n+=2;

	return n;

}

int luePikkumiessiirto(pikkumiessiirto *siirto, char *msg){
	int n=0;

	memcpy(&siirto->x,&msg[n],sizeof(float));
	n+=4;
	memcpy(&siirto->y,&msg[n],sizeof(float));
	n+=4;
	memcpy(&siirto->energia,&msg[n],sizeof(float));
	n+=4;

	memcpy(&siirto->aika,&msg[n],sizeof(short));
	n+=2;
	memcpy(&siirto->numero,&msg[n],sizeof(short));
	n+=2;

	return n;
}

int spistekoko=4;
int kirjotaSPiste(char *msg, SPiste *piste){
	int n=0;

	signed short tempx=piste->x*150.0f;
	signed short tempy=piste->y*150.0f;

	if(piste->x*150.0f>32760) tempx=32760;
	if(piste->x*150.0f<-32760) tempx=-32760;
	if(piste->y*150.0f>32760) tempy=32760;
	if(piste->y*150.0f<-32760) tempy=-32760;

	memcpy(&msg[n],&tempx,sizeof(short));
	n+=sizeof(short);
	memcpy(&msg[n],&tempy,sizeof(short));
	n+=sizeof(short);

	return n;
}
int lueSPiste(SPiste *piste, char *msg){
	int n=0;

	signed short tempx, tempy;

	memcpy(&tempx,&msg[n],sizeof(short));
	n+=sizeof(short);
	memcpy(&tempy,&msg[n],sizeof(short));
	n+=sizeof(short);


	piste->x=(float)tempx/150.0f;
	piste->y=(float)tempy/150.0f;

	return n;
}

int cpistekoko=6;
int kirjotaCPiste(char *msg, CPiste *piste){
	int n=0;

	signed short tempx=piste->x*150.0f;
	signed short tempy=piste->y*150.0f;

	if(piste->x*150.0f>32760) tempx=32760;
	if(piste->x*150.0f<-32760) tempx=-32760;
	if(piste->y*150.0f>32760) tempy=32760;
	if(piste->y*150.0f<-32760) tempy=-32760;

	memcpy(&msg[n],&tempx,sizeof(short));
	n+=sizeof(short);
	memcpy(&msg[n],&tempy,sizeof(short));
	n+=sizeof(short);

/*	memcpy(&msg[n],&piste->x,sizeof(float));
	n+=4;
	memcpy(&msg[n],&piste->y,sizeof(float));
	n+=4;
*/	
	char tempjuttu=piste->numero;
	if(piste->enabled) tempjuttu+=32;
	memcpy(&msg[n],&tempjuttu,sizeof(char));
	n+=1;

	//memcpy(&msg[n],&piste->player,sizeof(char));
	//n+=1;

	memcpy(&msg[n],&piste->player,sizeof(char));
	n+=1;

	return n;
}

int lueCPiste(CPiste *piste, char *msg){
	int n=0;

	signed short tempx, tempy;

	memcpy(&tempx,&msg[n],sizeof(short));
	n+=sizeof(short);
	memcpy(&tempy,&msg[n],sizeof(short));
	n+=sizeof(short);

	char tempjuttu;
	memcpy(&tempjuttu,&msg[n],sizeof(char));
	n+=1;
	if(tempjuttu&32){
		piste->enabled=true;
		tempjuttu-=32;
	}
	else piste->enabled=false;


	memcpy(&piste->player,&msg[n],sizeof(char));
	n+=1;

	piste->numero=tempjuttu;
	piste->x=(float)tempx/150.0f;
	piste->y=(float)tempy/150.0f;

	return n;

	
}

int limakoko=12;
int kirjotaLimajuttu(char *msg, limajuttu *pati){
	int n=0;
	memcpy(&msg[n],&pati->x,sizeof(float));
	n+=4;
	memcpy(&msg[n],&pati->y,sizeof(float));
	n+=4;

	memcpy(&msg[n],&pati->player,sizeof(short));
	n+=2;
	memcpy(&msg[n],&pati->kohta,sizeof(short));
	n+=2;

	return n;
}

int lueLimajuttu(limajuttu *pati, char *msg){
	int n=0;

	memcpy(&pati->x,&msg[n],sizeof(float));
	n+=4;
	memcpy(&pati->y,&msg[n],sizeof(float));
	n+=4;

	memcpy(&pati->player,&msg[n],sizeof(short));
	n+=2;
	memcpy(&pati->kohta,&msg[n],sizeof(short));
	n+=2;

	return n;
}

int kuolevapatikoko=2;
int kirjotaKuolevapati(char *msg, kuolevaPati *pati){
	int n=0;
	
	unsigned char temppati=pati->pati;

	memcpy(&msg[n],&temppati,sizeof(char));
	n+=sizeof(char);

	memcpy(&msg[n],&pati->pelaaja,sizeof(char));
	n+=sizeof(char);

	return n;
}

int lueKuolevapati(kuolevaPati *pati, char *msg){
	int n=0;

	unsigned char temppati;

	memcpy(&temppati,&msg[n],sizeof(char));
	n+=sizeof(char);

	memcpy(&pati->pelaaja,&msg[n],sizeof(char));
	n+=sizeof(char);

	pati->pati=temppati;

	return n;
}

int netpatikoko=15;
int kirjotaNetpati(char *msg, netPati *pati){
	int n=0;


	signed short tempx=pati->x*150.0f;
	signed short tempy=pati->y*150.0f;
	signed short tempxv=pati->xv*2000.0f;
	signed short tempyv=pati->yv*2000.0f;
	signed short tempkulma=pati->kulma*50.0f;
	unsigned char tempr=pati->r*255.0f;
	unsigned char tempg=pati->g*255.0f;
	unsigned char tempb=pati->b*255.0f;
	unsigned char temppati=pati->pati;
	//unsigned char temppelaaja=pati->pelaaja;

	if(pati->x*150.0f>32760) tempx=32760;
	if(pati->x*150.0f<-32760) tempx=-32760;
	if(pati->y*150.0f>32760) tempy=32760;
	if(pati->y*150.0f<-32760) tempy=-32760;

	if(pati->xv*2000.0f>32760) tempxv=32760;
	if(pati->xv*2000.0f<-32760) tempxv=-32760;
	if(pati->yv*2000.0f>32760) tempyv=32760;
	if(pati->yv*2000.0f<-32760) tempyv=-32760;


	if(pati->r*255.0f>255.0f) tempr=255;
	if(pati->g*255.0f>255.0f) tempg=255;
	if(pati->b*255.0f>255.0f) tempb=255;
	if(pati->r<0) tempr=0;
	if(pati->g<0) tempg=0;
	if(pati->b<0) tempb=0;

	memcpy(&msg[n],&tempx,sizeof(short));
	n+=sizeof(short);
	memcpy(&msg[n],&tempy,sizeof(short));
	n+=sizeof(short);
	memcpy(&msg[n],&tempxv,sizeof(short));
	n+=sizeof(short);
	memcpy(&msg[n],&tempyv,sizeof(short));
	n+=sizeof(short);
	memcpy(&msg[n],&tempkulma,sizeof(short));
	n+=sizeof(short);

	memcpy(&msg[n],&tempr,sizeof(char));
	n+=sizeof(char);
	memcpy(&msg[n],&tempg,sizeof(char));
	n+=sizeof(char);
	memcpy(&msg[n],&tempb,sizeof(char));
	n+=sizeof(char);

	memcpy(&msg[n],&temppati,sizeof(char));
	n+=sizeof(char);

	memcpy(&msg[n],&pati->pelaaja,sizeof(char));
	n+=sizeof(char);


	return n;
}

int lueNetpati(netPati *pati, char *msg){
	int n=0;

	signed short tempx;
	signed short tempy;
	signed short tempxv;
	signed short tempyv;
	signed short tempkulma;
	unsigned char tempr;
	unsigned char tempg;
	unsigned char tempb;
	unsigned char temppati;

	memcpy(&tempx,&msg[n],sizeof(short));
	n+=sizeof(short);
	memcpy(&tempy,&msg[n],sizeof(short));
	n+=sizeof(short);
	memcpy(&tempxv,&msg[n],sizeof(short));
	n+=sizeof(short);
	memcpy(&tempyv,&msg[n],sizeof(short));
	n+=sizeof(short);
	memcpy(&tempkulma,&msg[n],sizeof(short));
	n+=sizeof(short);

	memcpy(&tempr,&msg[n],sizeof(char));
	n+=sizeof(char);
	memcpy(&tempg,&msg[n],sizeof(char));
	n+=sizeof(char);
	memcpy(&tempb,&msg[n],sizeof(char));
	n+=sizeof(char);

	memcpy(&temppati,&msg[n],sizeof(char));
	n+=sizeof(char);

	memcpy(&pati->pelaaja,&msg[n],sizeof(char));
	n+=sizeof(char);

	pati->x=(float)tempx/150.0f;
	pati->y=(float)tempy/150.0f;
	pati->xv=(float)tempxv/2000.0f;
	pati->yv=(float)tempyv/2000.0f;
	pati->kulma=(float)tempkulma/50.0f;
	pati->r=(float)tempr/255.0f;
	pati->g=(float)tempg/255.0f;
	pati->b=(float)tempb/255.0f;
	pati->pati=temppati;


	//printf("pati tuli\n");

	return n;
}

int asteroidipoxkoko=8;
int kirjotaAsteroidipox(char *msg, asteroidipox *ast){
	int n=0;
	memcpy(&msg[n],&ast->x,sizeof(float));
	n+=4;
	memcpy(&msg[n],&ast->y,sizeof(float));
	n+=4;
	return n;
}

int lueAsteroidipox(asteroidipox *ast, char *msg){
	int n=0;
	memcpy(&ast->x,&msg[n],sizeof(float));
	n+=4;
	memcpy(&ast->y,&msg[n],sizeof(float));
	n+=4;
	return n;
}

int patikoko=48;
int kirjotaPati(char *msg, Pati *pati){
	int n=0;
	memcpy(&msg[n],&pati->x,sizeof(float));
	n+=4;
	memcpy(&msg[n],&pati->y,sizeof(float));
	n+=4;
	memcpy(&msg[n],&pati->xv,sizeof(float));
	n+=4;
	memcpy(&msg[n],&pati->yv,sizeof(float));
	n+=4;
	memcpy(&msg[n],&pati->kulma,sizeof(float));
	n+=4;
	memcpy(&msg[n],&pati->energia,sizeof(float));
	n+=4;
	memcpy(&msg[n],&pati->r,sizeof(float));
	n+=4;
	memcpy(&msg[n],&pati->g,sizeof(float));
	n+=4;
	memcpy(&msg[n],&pati->b,sizeof(float));
	n+=4;
	memcpy(&msg[n],&pati->alku,sizeof(float));
	n+=4;
	memcpy(&msg[n],&pati->koko,sizeof(float));
	n+=4;

	memcpy(&msg[n],&pati->aika,sizeof(short));
	n+=2;
	memcpy(&msg[n],&pati->ampumisviive,sizeof(short));
	n+=2;
	return n;
}

int luePati(Pati *pati, char *msg){
	int n=0;
	memcpy(&pati->x,&msg[n],sizeof(float));
	n+=4;
	memcpy(&pati->y,&msg[n],sizeof(float));
	n+=4;
	memcpy(&pati->xv,&msg[n],sizeof(float));
	n+=4;
	memcpy(&pati->yv,&msg[n],sizeof(float));
	n+=4;
	memcpy(&pati->kulma,&msg[n],sizeof(float));
	n+=4;
	memcpy(&pati->energia,&msg[n],sizeof(float));
	n+=4;
	memcpy(&pati->r,&msg[n],sizeof(float));
	n+=4;
	memcpy(&pati->g,&msg[n],sizeof(float));
	n+=4;
	memcpy(&pati->b,&msg[n],sizeof(float));
	n+=4;
	memcpy(&pati->alku,&msg[n],sizeof(float));
	n+=4;
	memcpy(&pati->koko,&msg[n],sizeof(float));
	n+=4;

	memcpy(&pati->aika,&msg[n],sizeof(short));
	n+=2;
	memcpy(&pati->ampumisviive,&msg[n],sizeof(short));
	n+=2;
	return n;
}

int asteroidikoko=10;
int kirjotaAsteroidi(char *msg, Pati *pati){
	int n=0;

	signed short xtemp=pati->x*100.0f;
	signed short ytemp=pati->y*100.0f;
	signed short xvtemp=pati->xv*1000.0f;
	signed short yvtemp=pati->yv*1000.0f;
	signed short tkoko=pati->koko*10000.0f;

	if(pati->x*100>32760) xtemp=32760;
	if(pati->y*100>32760) ytemp=32760;
	if(pati->y*100<-32760) ytemp=-32760;
	if(pati->x*100<-32760) xtemp=-32760;

	memcpy(&msg[n],&xtemp,sizeof(short));
	n+=sizeof(short);
	memcpy(&msg[n],&ytemp,sizeof(short));
	n+=sizeof(short);
	memcpy(&msg[n],&xvtemp,sizeof(short));
	n+=sizeof(short);
	memcpy(&msg[n],&yvtemp,sizeof(short));
	n+=sizeof(short);
	memcpy(&msg[n],&tkoko,sizeof(short));
	n+=sizeof(short);

	return n;
}

int lueAsteroidi(Pati *pati, char *msg){
	int n=0;

	signed short xtemp;
	signed short ytemp;
	signed short xvtemp;
	signed short yvtemp;
	signed short tkoko;

	memcpy(&xtemp,&msg[n],sizeof(short));
	n+=sizeof(short);
	memcpy(&ytemp,&msg[n],sizeof(short));
	n+=sizeof(short);
	memcpy(&xvtemp,&msg[n],sizeof(short));
	n+=sizeof(short);
	memcpy(&yvtemp,&msg[n],sizeof(short));
	n+=sizeof(short);
	memcpy(&tkoko,&msg[n],sizeof(short));
	n+=sizeof(short);

	pati->x=xtemp/100.0f;
	pati->y=ytemp/100.0f;
	pati->xv=xvtemp/1000.0f;
	pati->yv=yvtemp/1000.0f;
	pati->koko=tkoko/10000.0f;

	//pati->alku=random(10000)/10.0f;
	//pati->r=random(300)/100.0f-1.5f;
	//pati->g=random(300)/100.0f-1.5f;
	//pati->b=random(300)/100.0f-1.5f;
	pati->energia=asteroidienergia;

	return n;
}
