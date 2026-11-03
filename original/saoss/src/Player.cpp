#include "Define.h"
#include "main.h"
#include "Player.h"
#include "net.h"
#include "scene.h"
Player::Player(){
	easeviive=0;
	easekesto=0;
	kulma=0;
	x=0;y=0;
	while(sqrt(x*x+y*y)<15){
		x=random(maxx*2)-maxx;
		y=random(maxy*2)-maxy;
	}
	xvauhti=0;yvauhti=0;
	pylos=false; palas=false; pleft=false; pright=false; pfire=false;pease=false;
	suojapaalla=false;
	alpha=1.0f;
	kaasujalki=(Kaasujalki *) malloc(sizeof(Kaasujalki)*kaasumaara);
	kaasujalki2=(Kaasujalki *) malloc(sizeof(Kaasujalki)*kaasumaara);
	asekulma=0;
	propelli=0;
//	ampumisAaniKanava=1;
	fcounter=0;
	int tiimi=0;
}
Player::~Player(){
}

void Player::asetaAlus(Alus alus2){
	laseraani=false;
	suojapaalla=false;
	alus=alus2;
	energia=alus2.energia;
	fuel=alus2.fuel;
	alkusirpale=(Sirpale *) malloc(sizeof(Sirpale)*alkusirpaleita);
	memset(alkusirpale,0,sizeof(Sirpale)*alkusirpaleita);
//	if(alus.maxpadit>0){

	pati=(Pati *) malloc(sizeof(Pati)*maksimipadit);
	memset(pati, 0, sizeof(Pati)*maksimipadit);

//	}else pati=NULL;
	lima=(Pati *)malloc(sizeof(Pati)*maxlimat);
	memset(lima, 0, sizeof(Pati)*maxlimat);
//	if(alus.maxepadit>0){

	epati=(Pati *) malloc(sizeof(Pati)*maksimiepadit);
	memset(epati, 0, sizeof(Pati)*maksimiepadit);

//	}else epati=NULL;
//	int i;
/*	for(i=0;i<alus.maxepadit;i++){
		epati[i].aika=0;
	}*/
	viivat=new Viivakasa[maxviivat];
	memset(viivat, 0, sizeof(Viivakasa)*maxviivat);
	ampumispuoli=1;	
	teleportattu=false;
	limacount=0;
	ampumisjuttu=0;
	int i;
	for(i=0;i<palikkamaara;i++){
		xhanta[i]=x-alus.nopeus*i;
		yhanta[i]=y;
		kulmahanta[i]=kulma;
	}
	if(alus.ampumisaani!=NULL) ampumisaani=new Sound(alus.ampumisaani);
	else ampumisaani=NULL;
	if(alus.eampumisaani!=NULL) eampumisaani=new Sound(alus.eampumisaani);
	else eampumisaani=NULL;
	if(alus.osumisaani!=NULL) osumisaani=new Sound(alus.osumisaani);
	else osumisaani=NULL;
	if(alus.eosumisaani!=NULL) eosumisaani=new Sound(alus.eosumisaani);
	else eosumisaani=NULL;

}

void Player::uudestaan(Alus alus2){
	//if(epati!=NULL) free(epati);
	//if(pati!=NULL) free(pati);
//	free(viivat);
//	free(lima);

	
	ampumisviive=40;
	easeviive=20;
	alus=alus2;
	kaasujalkiaika=10;
	int tempx=x, tempy=y;
	x=0;y=0;
	xvauhti=0;yvauhti=0;

	while(sqrt(x*x+y*y)<planeettasade*10){
		x=(random(maxx*20)-maxx*10)/10.0f;
		y=(random(maxy*20)-maxy*10)/10.0f;
	}
	energia=alus.energia;
	fuel=alus.fuel;
//	lima=new Pati[maxlimat];
	int i;
//	if(alus.maxpadit>0){
//		pati=new Pati[(int) alus.maxpadit];
		for(i=0;i<alus.maxpadit;i++){
			pati[i].aika=0;
		}
//	}else pati=NULL;
	for(i=0;i<maxlimat;i++){
		lima[i].aika=0;
	}
	limacount=0;
//	if(alus.maxepadit>0){
//		epati=new Pati[(int) alus.maxepadit];
		for(i=0;i<alus.maxepadit;i++){
			epati[i].aika=0;
		}
//	}else epati=NULL;
//	viivat=new Viivakasa[maxviivat];
	for(i=0;i<maxviivat;i++){
		viivat[i].aika=0;
		viivat[i].x1=100;
	}

	ampumispuoli=1;	
	teleportattu=false;
	suojapaalla=false;
	ampumisjuttu=0;
	fcounter=0;
	for(i=0;i<palikkamaara;i++){
		xhanta[i]=x-alus.nopeus*i;
		yhanta[i]=y;
		kulmahanta[i]=kulma;
	}
	
	if(ampumisaani!=NULL) delete ampumisaani;
	if(eampumisaani!=NULL) delete eampumisaani;
	if(osumisaani!=NULL) delete osumisaani;
	if(eosumisaani!=NULL) delete eosumisaani;


	if(alus.ampumisaani!=NULL) ampumisaani=new Sound(alus.ampumisaani);
	else ampumisaani=NULL;
	if(alus.eampumisaani!=NULL) eampumisaani=new Sound(alus.eampumisaani);
	else eampumisaani=NULL;
	if(alus.osumisaani!=NULL) osumisaani=new Sound(alus.osumisaani);
	else osumisaani=NULL;
	if(alus.eosumisaani!=NULL) eosumisaani=new Sound(alus.eosumisaani);
	else eosumisaani=NULL;



}

void Player::laitaAlkuSirpaleet(){
	int i, rtemp;
	for(i=0;i<alkusirpaleita;i++){
		alkusirpale[i].x=100;
		alkusirpale[i].y=100;
		alkusirpale[i].z=100;
		float sade=alkusirpale[i].x*alkusirpale[i].x+alkusirpale[i].y*alkusirpale[i].y
			  +alkusirpale[i].z*alkusirpale[i].z;
		while(sade<1000 || sade>1600){
			alkusirpale[i].x=random(16000)/200.0f-40.0f;
			alkusirpale[i].y=random(16000)/200.0f-40.0f;
			alkusirpale[i].z=random(16000)/200.0f-40.0f;
			sade=alkusirpale[i].x*alkusirpale[i].x+alkusirpale[i].y*alkusirpale[i].y
			  +alkusirpale[i].z*alkusirpale[i].z;
		}
		alkusirpale[i].koko=random(250)/100.0f+0.8f;
		alkusirpale[i].muoto=0;
		switch(alus.numero){
		case 0:
			rtemp=random(100);
			if(rtemp<80){
				alkusirpale[i].red=random(90)/100.0f+0.6;
				alkusirpale[i].green=random(80)/100.0f+0.4f;
				alkusirpale[i].blue=random(50)/100.0f+0.3f;
			}else{
				alkusirpale[i].red=random(20)/100.0f+0.6;
				alkusirpale[i].green=random(20)/100.0f+0.6f;
				alkusirpale[i].blue=random(20)/100.0f+0.6f;
			}
			break;
		case 1:
			rtemp=random(100);
			if(rtemp<33){
				alkusirpale[i].red=random(30)/100.0f+0.6;
				alkusirpale[i].green=random(30)/100.0f+0.4f;
				alkusirpale[i].blue=random(20)/100.0f+0.0f;
			}else if(rtemp<66){
				alkusirpale[i].red=random(20)/100.0f+0.8;
				alkusirpale[i].green=random(30)/100.0f+0.4f;
				alkusirpale[i].blue=random(20)/100.0f+0.8f;
			}else{
				alkusirpale[i].red=random(20)/100.0f+0.8;
				alkusirpale[i].green=random(20)/100.0f+0.8f;
				alkusirpale[i].blue=random(20)/100.0f+0.8f;
			}
			break;
		case 2:
			rtemp=random(100);
			if(rtemp<33){
				alkusirpale[i].red=random(30)/100.0f+0.7;
				alkusirpale[i].green=random(20)/100.0f+0.0f;
				alkusirpale[i].blue=random(20)/100.0f+0.0f;
			}else if(rtemp<66){
				alkusirpale[i].red=random(30)/100.0f+0.7;
				alkusirpale[i].green=random(30)/100.0f+0.7f;
				alkusirpale[i].blue=random(20)/100.0f+0.0f;
			}else{
				alkusirpale[i].red=random(50)/100.0f+0.5;
				alkusirpale[i].green=random(30)/100.0f+0.0f;
				alkusirpale[i].blue=random(30)/100.0f+0.7f;
			}
			break;
		case 3:
			rtemp=random(100);
			if(rtemp<60){
				alkusirpale[i].red=random(10)/100.0f+0.45;
				alkusirpale[i].green=random(7)/100.0f+0.36f;
				alkusirpale[i].blue=random(7)/100.0f+0.36f;
			}else if(rtemp<80){
				alkusirpale[i].red=random(30)/100.0f+0.7;
				alkusirpale[i].green=random(20)/100.0f+0.0f;
				alkusirpale[i].blue=random(20)/100.0f+0.0f;
			}else{
				alkusirpale[i].red=random(20)/100.0f+0.7;
				alkusirpale[i].green=random(20)/100.0f+0.7f;
				alkusirpale[i].blue=random(20)/100.0f+0.7f;
			}
			break;
		case 4:
			rtemp=random(100);
			if(rtemp<33){
				alkusirpale[i].red=random(30)/100.0f+0.7;
				alkusirpale[i].green=random(30)/100.0f+0.7f;
				alkusirpale[i].blue=random(30)/100.0f+0.7f;
			}else if(rtemp<66){
				alkusirpale[i].red=random(30)/100.0f+0.7;
				alkusirpale[i].green=random(30)/100.0f+0.7f;
				alkusirpale[i].blue=random(20)/100.0f+0.0f;
			}else{
				alkusirpale[i].red=random(20)/100.0f+0.0;
				alkusirpale[i].green=random(20)/100.0f+0.0f;
				alkusirpale[i].blue=random(30)/100.0f+0.7f;
			}
			break;
		case 5:
			rtemp=random(100);
			if(rtemp<33){
				alkusirpale[i].red=random(30)/100.0f+0.6;
				alkusirpale[i].green=random(30)/100.0f+0.6f;
				alkusirpale[i].blue=random(30)/100.0f+0.6f;
			}else if(rtemp<66){
				alkusirpale[i].red=random(30)/100.0f+0.7;
				alkusirpale[i].green=random(30)/100.0f+0.4f;
				alkusirpale[i].blue=random(30)/100.0f+0.2f;
			}else{
				alkusirpale[i].red=random(50)/100.0f+0.5;
				alkusirpale[i].green=random(50)/100.0f+0.3f;
				alkusirpale[i].blue=random(50)/100.0f+0.1f;
			}
			break;
		case 6:
			rtemp=random(100);
			alkusirpale[i].red=random(100)/400.0f;
			alkusirpale[i].green=random(100)/400.0f;
			alkusirpale[i].blue=random(100)/400.0f+0.75f;
			break;
		case 7:
			rtemp=random(100);
			if(rtemp<66){
				alkusirpale[i].red=random(20)/100.0f+0.7;
				alkusirpale[i].green=alkusirpale[i].red;
				alkusirpale[i].blue=alkusirpale[i].red;
			}else if(rtemp<88){
				alkusirpale[i].red=random(30)/100.0f+0.7;
				alkusirpale[i].green=random(20)/100.0f+0.0f;
				alkusirpale[i].blue=random(20)/100.0f+0.0f;
			}else{
				alkusirpale[i].red=random(30)/100.0f+0.7;
				alkusirpale[i].green=random(30)/100.0f+0.7f;
				alkusirpale[i].blue=random(20)/100.0f+0.0f;
			}
			break;
		case 8:
			rtemp=random(100);
			if(rtemp<33){
				alkusirpale[i].red=random(20)/100.0f+0.7;
				alkusirpale[i].green=random(10)/100.0f+0.0f;
				alkusirpale[i].blue=random(20)/100.0f+0.4f;
			}else {
				alkusirpale[i].red=random(20)/100.0f+0.6;
				alkusirpale[i].green=random(20)/100.0f+0.5f;
				alkusirpale[i].blue=random(20)/100.0f+0.45f;
			}
			break;
		case 9:
			alkusirpale[i].red=random(100)/400.0f;
			alkusirpale[i].green=random(100)/400.0f+0.75f;
			alkusirpale[i].blue=random(100)/400.0f;
			break;
		case 10:
			alkusirpale[i].red=random(30)/100.0f+0.7;
			alkusirpale[i].green=random(15)/100.0f+0.2f;
			alkusirpale[i].blue=0;
			break;
		case 11:
			alkusirpale[i].red=random(40)/100.0f+0.6;
			alkusirpale[i].green=random(40)/100.0f+0.6f;
			alkusirpale[i].blue=random(40)/100.0f+0.6f;
			break;
		case 12:
			rtemp=random(100);
			if(rtemp<66){
				alkusirpale[i].red=random(15)/100.0f+0.0;
				alkusirpale[i].green=random(15)/100.0f+0.0f;
				alkusirpale[i].blue=random(40)/100.0f+0.6f;
			}else {
				alkusirpale[i].red=random(20)/100.0f+0.6;
				alkusirpale[i].green=alkusirpale[i].red;
				alkusirpale[i].blue=alkusirpale[i].red;
			}
			break;
		case 13:
			alkusirpale[i].red=random(5)/100.0f;
			alkusirpale[i].green=random(60)/100.0f+0.3f;
			alkusirpale[i].blue=random(5)/100.0f;
			break;
		case 14:
			rtemp=random(100);
			if(rtemp<60){
				alkusirpale[i].red=random(50)/100.0f+0.1;
				alkusirpale[i].green=alkusirpale[i].red;
				alkusirpale[i].blue=random(100.0f*alkusirpale[i].red)/100.0f+alkusirpale[i].red/10.0f;
			}else if(rtemp<85){
				alkusirpale[i].red=random(20)/100.0f+0.8;
				alkusirpale[i].green=random(40)/100.0f+0.0f;
				alkusirpale[i].blue=random(40)/100.0f+0.0f;
			}else{
				alkusirpale[i].red=0;
				alkusirpale[i].green=random(50)/100.0f+0.5f;
				alkusirpale[i].blue=0;
			}
			break;
		case 15:
			rtemp=random(100);
			if(rtemp<50){
				alkusirpale[i].blue=random(50)/100.0f+0.5f;
				alkusirpale[i].red=random(100.0f*alkusirpale[i].blue/2)/100.0f+0.1;
				alkusirpale[i].green=random(100.0f*alkusirpale[i].blue/1.5)/100.0f+0.1;
			}else if(rtemp<80){
				alkusirpale[i].red=random(20)/100.0f+0.5f;
				alkusirpale[i].green=random(10)/100.0f+0.0f;
				alkusirpale[i].blue=random(10)/100.0f+0.0f;
			}else{
				alkusirpale[i].red=random(20)/100.0f+0.8f;
				alkusirpale[i].green=random(50)/100.0f+0.1f;
				alkusirpale[i].blue=random(30)/100.0f+0.4f;
			}
			break;
		case 16:
			rtemp=random(100);
			if(rtemp<50){
				alkusirpale[i].blue=random(10)/100.0f+0.05f;
				alkusirpale[i].red=random(10)/100.0f+0.05f;
				alkusirpale[i].green=random(30)/100.0f+0.7;
			}else if(rtemp<85){
				alkusirpale[i].red=random(30)/100.0f+0.7f;
				alkusirpale[i].green=random(10)/100.0f+0.05f;
				alkusirpale[i].blue=random(10)/100.0f+0.05f;
			}else{
				alkusirpale[i].red=random(10)/100.0f+0.05f;
				alkusirpale[i].green=random(10)/100.0f+0.05f;
				alkusirpale[i].blue=random(30)/100.0f+0.7f;
			}
			break;
		case 17:
			rtemp=random(100);
			if(rtemp<60){
				alkusirpale[i].blue=random(30)/100.0f+0.5f;
				alkusirpale[i].red=random(30)/100.0f+0.5f;
				alkusirpale[i].green=random(30)/100.0f+0.5f;
			}else if(rtemp<75){
				alkusirpale[i].red=random(30)/100.0f+0.7f;
				alkusirpale[i].green=random(10)/100.0f+0.05f;
				alkusirpale[i].blue=random(10)/100.0f+0.05f;
			}else{
				alkusirpale[i].red=random(10)/100.0f+0.05f;
				alkusirpale[i].green=random(10)/100.0f+0.05f;
				alkusirpale[i].blue=random(30)/100.0f+0.7f;
			}
			break;
		case 18:
			rtemp=random(100);
			if(rtemp<70){
				alkusirpale[i].blue=random(10)/100.0f+0.0f;
				alkusirpale[i].red=random(30)/100.0f+0.7f;
				alkusirpale[i].green=random(10)/100.0f+0.0f;
			}else{
				alkusirpale[i].red=random(20)/100.0f+0.5f;
				alkusirpale[i].green=random(20)/100.0f+0.5f;
				alkusirpale[i].blue=random(20)/100.0f+0.5f;
			}
			break;
		case 19:
			rtemp=random(100);
			if(rtemp<80){
				alkusirpale[i].blue=random(30)/100.0f+0.7f;
				alkusirpale[i].red=random(20)/100.0f+0.1f;
				alkusirpale[i].green=random(20)/100.0f+0.1f;
			}else{
				alkusirpale[i].red=random(30)/100.0f+0.7f;
				alkusirpale[i].green=random(10)/100.0f+0.0f;
				alkusirpale[i].blue=random(10)/100.0f+0.0f;
			}
			break;
		case 20:
			alkusirpale[i].blue=random(20)/100.0f+0.1f;
			alkusirpale[i].green=random(40)/100.0f+0.6f;
			alkusirpale[i].red=random(20)/100.0f+0.1f;
			break;
		case 21:
			rtemp=random(100);
			if(rtemp<35){
				alkusirpale[i].blue=random(20)/100.0f+0.35f;
				alkusirpale[i].red=random(20)/100.0f+0.5f;
				alkusirpale[i].green=random(20)/100.0f+0.35f;
			}else if(rtemp<75){
				alkusirpale[i].red=random(10)/100.0f+0.15f;
				alkusirpale[i].green=random(30)/100.0f+0.7f;
				alkusirpale[i].blue=random(10)/100.0f+0.15f;
			}else{
				alkusirpale[i].red=random(30)/100.0f+0.7f;
				alkusirpale[i].green=random(10)/100.0f+0.05f;
				alkusirpale[i].blue=random(10)/100.0f+0.05f;
			}
			break;
		case 22:
			rtemp=random(100);
			if(rtemp<90){
				alkusirpale[i].blue=random(20)/100.0f+0.1f;
				alkusirpale[i].red=random(20)/100.0f+0.1f;
				alkusirpale[i].green=random(70)/100.0f+0.3f;
			}else{
				alkusirpale[i].red=random(30)/100.0f+0.7f;
				alkusirpale[i].green=random(10)/100.0f+0.0f;
				alkusirpale[i].blue=random(10)/100.0f+0.0f;
			}
			break;
		case 23:
			alkusirpale[i].blue=random(20)/100.0f+0.6f;
			alkusirpale[i].green=random(35)/100.0f+0.6f;
			alkusirpale[i].red=random(20)/100.0f+0.6f;
			break;
		default:
			alkusirpale[i].red=random(100)/400.0f;
			alkusirpale[i].green=random(100)/400.0f;
			alkusirpale[i].blue=random(100)/400.0f+0.75f;
			break;
		}
		alkusirpale[i].aika=syntymisaika;
		alkusirpale[i].maxaika=syntymisaika;

	}
}
void Player::laskeAlkuSirpaleet(){
	int i;
	for(i=0;i<alkusirpaleita;i++){
		if(alkusirpale[i].aika>0){
			alkusirpale[i].aika--;
			double kulma2=atan2(alkusirpale[i].y,alkusirpale[i].x);
			double etaisyys=sqrt(alkusirpale[i].x*alkusirpale[i].x+alkusirpale[i].y*alkusirpale[i].y);
			kulma2+=(PI/180.0f)*4.0f;
			alkusirpale[i].x=cos(kulma2)*etaisyys;
			alkusirpale[i].y=sin(kulma2)*etaisyys;
			alkusirpale[i].x/=1.0165;
			alkusirpale[i].y/=1.0165;
			alkusirpale[i].z/=1.0165;
		}
	}
	if(kuolema==1){
		kuolema=0;
		int scount=alkusirpaleita;
		int j=0;
		while(j<sirpaleita && scount>0){
			if(sirpale[j].aika<=0){
				scount--;
				sirpale[j].x=x;
				sirpale[j].y=y;
				sirpale[j].z=0;
				sirpale[j].xv=1;
				sirpale[j].yv=1;
				sirpale[j].zv=1;
//				while(sqrt(sirpale[j].xv*sirpale[j].xv+sirpale[j].yv*sirpale[j].yv)>0.25f){
				while(sirpale[j].xv*sirpale[j].xv+sirpale[j].yv*sirpale[j].yv
					  +sirpale[j].zv*sirpale[j].zv>0.25f){
					sirpale[j].xv=random(10000)/10000.0f-0.5f;
					sirpale[j].yv=random(10000)/10000.0f-0.5f;
					sirpale[j].zv=random(10000)/10000.0f-0.5f;
				}
				sirpale[j].maxaika=200;
				sirpale[j].aika=random(150)+50;
				sirpale[j].koko=random(180)/100.0f+0.35;
				sirpale[j].muoto=0;
				sirpale[j].red=alkusirpale[scount].red;
				sirpale[j].green=alkusirpale[scount].green;
				sirpale[j].blue=alkusirpale[scount].blue;
			}
			j++;
		}
	}
}

void Player::piirraAlkuSirpaleet(){
	glDisable(GL_DEPTH_TEST);			
	glEnable(GL_BLEND);
	glBlendFunc(GL_SRC_ALPHA, GL_ONE);
//	glBlendFunc(GL_ONE,GL_ONE);	
	int i;
	for(i=0;i<alkusirpaleita;i++){
		if(alkusirpale[i].aika>0){
			double alpha2;
/*			if(alkusirpale[i].aika<=alkusirpale[i].maxaika/2)
				alpha2=(double)alkusirpale[i].aika/(double)alkusirpale[i].maxaika/(double)2+0.6f;
			else alpha2=1.0f-((double)alkusirpale[i].aika-(double)alkusirpale[i].maxaika/(double)2)/(double)alkusirpale[i].maxaika/(double)2+0.6f;*/
			alpha2=1.0f-((double)alkusirpale[i].aika)/(double)alkusirpale[i].maxaika;
			alpha2=1.0f-alpha2*alpha2;
//			alpha2=1;
			glPushMatrix();
			glTranslatef(x,y,alustaso);
//			glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);
			glTranslatef(alkusirpale[i].x,alkusirpale[i].y,alkusirpale[i].z);
			glBindTexture(GL_TEXTURE_2D, texture[18]);
			glColor4f(alkusirpale[i].red,alkusirpale[i].green,alkusirpale[i].blue,alpha2);		
			double limakoko=alkusirpale[i].koko;
	
			glBegin(GL_QUADS);
				glNormal3f(0,0,1.0f);
				glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*limakoko, -1.0f*limakoko,  1.0f);	// Bottom Left Of The Texture and Quad
				glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*limakoko, -1.0f*limakoko,  1.0f);	// Bottom Right Of The Texture and Quad
				glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*limakoko,  1.0f*limakoko,  1.0f);	// Top Right Of The Texture and Quad
				glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*limakoko,  1.0f*limakoko,  1.0f);	// Top Left Of The Texture and Quad
			glEnd();

			glPopMatrix();
		}
	}
	glBlendFunc(GL_SRC_COLOR,GL_ONE_MINUS_SRC_COLOR);

}
void Player::lisaalima(){
	if(limacount<maxlimat){
		lima[limacount].aika=400;
		lima[limacount].x=random(1500)/1000.0f*alus.sade*0.785-0.75f;
		lima[limacount].y=random(1500)/1000.0f*alus.sade*0.785-0.75f;
		if(hostpeli){
			limalaitetaan=true;
			netlima.x=lima[limacount].x;
			netlima.y=lima[limacount].y;
			netlima.player=numero;
			netlima.kohta=limacount;
		}
		limacount++;
	}
}

void Player::piirraLimat(){
	glDisable(GL_DEPTH_TEST);			
	glEnable(GL_BLEND);
	glBlendFunc(GL_ONE,GL_ONE);	

	int i;
	double limakoko=0.5;
	for(i=0;i<maxlimat;i++){
		if(lima[i].aika>0){
			glPushMatrix();
			glTranslatef(x,y,alustaso);
			glRotatef(kulma-90.0f,0.0f,0.0f,1.0f);
			glTranslatef(lima[i].x,lima[i].y,0);
			glBindTexture(GL_TEXTURE_2D, texture[18]);
			glColor4f(0.1f,0.7f,0.1f,1.0f);		
	
			glBegin(GL_QUADS);
				glNormal3f(0,0,1.0f);
				glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*limakoko, -1.0f*limakoko,  1.0f);	// Bottom Left Of The Texture and Quad
				glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*limakoko, -1.0f*limakoko,  1.0f);	// Bottom Right Of The Texture and Quad
				glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*limakoko,  1.0f*limakoko,  1.0f);	// Top Right Of The Texture and Quad
				glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*limakoko,  1.0f*limakoko,  1.0f);	// Top Left Of The Texture and Quad
			glEnd();

			glPopMatrix();
		}
	}
	glBlendFunc(GL_SRC_COLOR,GL_ONE_MINUS_SRC_COLOR);

}
void Player::piirraGraffat(){
	glPushMatrix();
	glEnable(GL_BLEND);
	glDisable(GL_DEPTH);
	glBlendFunc(GL_ONE,GL_ONE);	
	glDisable(GL_LIGHTING);
	
	//glTranslatef(camposx,camposy,camposz-30.0f);

	float minx2;
	float maxx2;
	minx2=-14.5f;
	maxx2=12.5f;
	if(players<11){
		if(numero==0){
			glTranslatef(minx2, +10.0f, 0);
		}else if(numero==1){
			glTranslatef(maxx2, +10.0f, 0);
		}else if(numero==2){
			glTranslatef(minx2, -10.0f, 0);
		}else if(numero==3){
			glTranslatef(maxx2, -10.0f, 0);
		}
		else if(numero==4){
			glTranslatef(minx2, +6.5f, 0);
		}else if(numero==5){
			glTranslatef(minx2, +3.0f, 0);
		}else if(numero==6){
			glTranslatef(minx2, -2.5f, 0);
		}else if(numero==7){
			glTranslatef(minx2, -6.0f, 0);
		}else if(numero==8){
			glTranslatef(maxx2, +6.5f, 0);
		}else if(numero==9){
			glTranslatef(maxx2, -6.0f, 0);
		}
	}else{
		if(numero<8){
			glTranslatef(minx2, +10.0f-numero*2.85, 0);
		}else{
			glTranslatef(maxx2, +10.0f-(numero-8)*2.85,0);
		}
	}
	char numero1[3];
	char numero2[3];
	char numero3[8];
	char numero4[8];
	itoa(numero+1			   ,numero1,10);	
	itoa(player[numero].tiimi+1,numero2,10);	
	itoa(               pisteet,numero3,10);	
	if(!joinpeli && !hostpeli){
		itoa(fullcredits           ,numero4,10);	
	}else 
		itoa(mpcredits             ,numero4,10);	


	float kerroin=1.0f/((float) screenwidth/512.0f);
	kerroin=sqrt(kerroin);
	glScalef(kerroin,kerroin,kerroin);
	//player x/nimi
	valitseVari(player[numero].tiimi);	
	if(!joinpeli && !hostpeli){
		gl_println(-1.0, 1,		0,"player",	0.3);
		gl_println( 1.0, 1,		0, numero1,	0.3);
	}else{
		gl_println(-1.0, 1,		0,netnick[numero],	0.3);
	}

	//team numero
	if(numero<9){
		gl_println( 2.0, 1,		0,"team",	0.3);
		gl_println( 3.7, 1,		0, numero2,	0.3);
	}else{
		gl_println( 2.0, 1,		0,"team",	0.3);
		gl_println( 3.7, 1,		0, numero2,	0.3);
	}

	gl_println(	 -1.0, 0.33,	0,"shield",	0.3);

	gl_println(-0.4,-0.125,	0,"fuel",	0.3);
	if(pistepeli || 
		((joinpeli || hostpeli) && (gamemode==DEATHMATCH || gamemode==DEATHMATCHRANDOM))){
		gl_println(-1.5,-0.6,  0,"credits",0.25);
		gl_println( 1  ,-0.6,	0, numero3,	0.25);
		gl_println( 2.6,-0.6,	0, "/",		0.25);
		gl_println( 2.8,-0.6,	0, numero4,	0.25);
	}



	glTranslatef(2.0f,0,0);
	glDisable(GL_TEXTURE_2D);
	if(energia<0)energia=0;
	glColor4f(0.25,0.25,0.0,0.5);
	glBegin(GL_QUADS);		
		glVertex3f(-1.05f,	0.20f,  0.0f);	
		glVertex3f( 1.05f,	0.20f,  0.0f);	
		glVertex3f( 1.05f,	0.46f,  0.0f);	
		glVertex3f(-1.05f,	0.46f,  0.0f);	

		glVertex3f(-1.05f,	-0.25f,  0.0f);	
		glVertex3f( 1.05f,	-0.25f,  0.0f);	
		glVertex3f( 1.05f,	 0.01f,  0.0f);	
		glVertex3f(-1.05f,	 0.01f,  0.0f);	
	glEnd();


//	glColor4f(1.0f-(energia/alus.energia),energia/alus.energia,0.0f, 0.8f);		
	glBegin(GL_QUADS);		
		glColor4f(1.0f-(pow(energia/alus.energia,1.7)),energia/alus.energia,0.0f, 0.8f);		
		glVertex3f(-1.0f,							0.23f,  0.0f);	
		glVertex3f(-1.0f+energia/alus.energia*2.0f,	0.23f,  0.0f);	
		glVertex3f(-1.0f+energia/alus.energia*2.0f,	0.43f,  0.0f);	
		glVertex3f(-1.0f,							0.43f,  0.0f);	

		glColor4f(1.0f-(pow(fuel/alus.fuel,1.7)),fuel/alus.fuel,0.0f, 0.8f);		
		glVertex3f(-1.0f,						-0.22f,  0.0f);	
		glVertex3f(-1.0f+fuel/alus.fuel*2.0f,	-0.22f,  0.0f);	
		glVertex3f(-1.0f+fuel/alus.fuel*2.0f,	-0.02f,  0.0f);	
		glVertex3f(-1.0f,						-0.02f,  0.0f);	
	glEnd();
//	glEnable(GL_LIGHTING);
	glColor4f(0.2,0.5,0.2,0.5);
/*	glLineWidth(1.0f);
	glBegin(GL_LINES);		
		glVertex3f(-1.06,0.20,0);
		glVertex3f( 1.06,0.20,0);
		glVertex3f( 1.03,0.26,0);
		glVertex3f( 1.03,0.40,0);
		glVertex3f( 1.06,0.46,0);
		glVertex3f(-1.06,0.46,0);
		glVertex3f(-1.03,0.40,0);
		glVertex3f(-1.03,0.26,0);
	glEnd();*/

	glEnable(GL_TEXTURE_2D);
	
	glPopMatrix();

}

void Player::laskeViivat(){
	int i;
	for(i=0;i<maxviivat;i++){
		if(viivat[i].aika>0){
			viivat[i].aika--;
			viivat[i].x1+=viivat[i].xv;
			viivat[i].x2+=viivat[i].xv;
			viivat[i].y1+=viivat[i].yv;
			viivat[i].y2+=viivat[i].yv;
		}
	}
}

void Player::piirraViivat(){
	glDisable(GL_TEXTURE_2D);
	int i;
	glEnable(GL_LINE_SMOOTH);
	float kertojuttu=1;

	if(alus.aselaji==6 || alus.aselaji==9 || alus.aselaji==11) 
		glLineWidth(2.0f*kertojuttu);
	else glLineWidth(1.0f);
	if(alus.numero==18){
		glLineWidth(1.5f);
	}

	for(i=0;i<maxviivat;i++){
		if(viivat[i].aika>0){
			glPushMatrix();
				glTranslatef(viivat[i].x1,viivat[i].y1,alustaso);
				glColor4f(viivat[i].r,viivat[i].g,viivat[i].b, 0.9f);		
				glBegin(GL_LINES);		
					glVertex3f(0,0,0);
					glVertex3f(viivat[i].x2-viivat[i].x1,viivat[i].y2-viivat[i].y1,0);	
				glEnd();
			glPopMatrix();
		}
	}
//	glEnable(GL_LIGHTING);

	glEnable(GL_TEXTURE_2D);
}

void Player::laskeKaasujaljet(){
	int i;
	if(kaasujalkiaika<1){
		kaasujalkiaika=10;
		if(kaasujalki[0].alpha>=1){
			if(alus.numero==4){
				kaasujalki[0].x=x+alus.sade*cos((kulma-145.0f)/180.0f*PI);
				kaasujalki[0].y=y+alus.sade*sin((kulma-145.0f)/180.0f*PI);
				kaasujalki2[0].x=x+alus.sade*cos((kulma-215.0f)/180.0f*PI);
				kaasujalki2[0].y=y+alus.sade*sin((kulma-215.0f)/180.0f*PI);
			}else if(alus.numero==23){
				kaasujalki[0].x=x+alus.sade*cos((kulma-155.0f)/180.0f*PI);
				kaasujalki[0].y=y+alus.sade*sin((kulma-155.0f)/180.0f*PI);
				kaasujalki2[0].x=x+alus.sade*cos((kulma-205.0f)/180.0f*PI);
				kaasujalki2[0].y=y+alus.sade*sin((kulma-205.0f)/180.0f*PI);
			}else{
				kaasujalki[0].x=x+alus.sade*cos((kulma-180.0f)/180.0f*PI);
				kaasujalki[0].y=y+alus.sade*sin((kulma-180.0f)/180.0f*PI);
			}
		}else {
			kaasujalki[0].alpha=0;
			if(alus.numero==4 || alus.numero==23){
				kaasujalki2[0].alpha=0;
			}
		}
		for(i=kaasumaara-1;i>0;i--){
			if(kaasujalki[i-1].alpha>0) kaasujalki[i].alpha=1-(double)(i+1)/(double)kaasumaara;
			else kaasujalki[i].alpha=0;
			kaasujalki[i].x=kaasujalki[i-1].x;
			kaasujalki[i].y=kaasujalki[i-1].y;
			if(alus.numero==4 || alus.numero==23){
				if(kaasujalki2[i-1].alpha>0)kaasujalki2[i].alpha=1-(double)(i+1)/(double)kaasumaara;
				else kaasujalki2[i].alpha=0;
				kaasujalki2[i].x=kaasujalki2[i-1].x;
				kaasujalki2[i].y=kaasujalki2[i-1].y;
			}
		}
		kaasujalki[0].alpha-=0.01;
		if(alus.numero==4 || alus.numero==23){
			kaasujalki2[0].alpha-=0.01;
		}
	}else kaasujalkiaika--;
/*	if(kaasujalkiaika<1){
		kaasujalkiaika=10;
		for(i=kaasumaara-1;i>0;i--){
			if(pylos){
				kaasujalki[i].x=kaasujalki[i-1].x;
				kaasujalki[i].y=kaasujalki[i-1].y;
			}
			kaasujalki[i].alpha=kaasujalki[i-1].alpha-1.0f/kaasumaara;
		}
		if(alus.numero==4){
			for(i=kaasumaara-1;i>0;i--){
				kaasujalki2[i].alpha=kaasujalki2[i-1].alpha-1.0f/kaasumaara;
				if(pylos){
					kaasujalki2[i].x=kaasujalki2[i-1].x;
					kaasujalki2[i].y=kaasujalki2[i-1].y;
				}
			}
			if(pylos){
				kaasujalki[0].x=x+alus.sade*cos((kulma-150.0f)/180.0f*PI);
				kaasujalki[0].y=y+alus.sade*sin((kulma-150.0f)/180.0f*PI);
				kaasujalki2[0].x=x+alus.sade*cos((kulma-210.0f)/180.0f*PI);
				kaasujalki2[0].y=y+alus.sade*sin((kulma-210.0f)/180.0f*PI);
			}
		}else{
			if(pylos){
				kaasujalki[0].x=x+alus.sade*cos((kulma-180.0f)/180.0f*PI);
				kaasujalki[0].y=y+alus.sade*sin((kulma-180.0f)/180.0f*PI);
			}
		}
		
		if(kaasujalki[0].alpha>0) kaasujalki[0].alpha-=1.0f/kaasumaara;
		if(alus.numero==4){
			if(kaasujalki2[0].alpha>0) kaasujalki2[0].alpha-=1.0f/kaasumaara;
		}
	}else kaasujalkiaika--;*/
}

void Player::piirraKaasujaljet(){
//	glBlendFunc(GL_SRC_ALPHA,GL_ONE_MINUS_SRC_COLOR);
	glDisable(GL_DEPTH_TEST);			
	glEnable(GL_BLEND);
	glEnable(GL_TEXTURE_2D);
	glBlendFunc(GL_ONE,GL_ONE);	

	int i;
	if(alus.numero!=16 && alus.numero!=22) for(i=0;i<kaasumaara;i++){
		if(kaasujalki[i].alpha>0.0f){
			glPushMatrix();
			glTranslatef(kaasujalki[i].x,kaasujalki[i].y,alustaso);
//			glBindTexture(GL_TEXTURE_2D, texture[6]);
			glBindTexture(GL_TEXTURE_2D, texture[18]);
			glColor4f(1.0f,0.0f,0.0f,kaasujalki[i].alpha);		
	
			glBegin(GL_QUADS);
				glNormal3f(0,0,1.0f);
				glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*alus.jalkikoko*2, -1.0f*alus.jalkikoko*2,  1.0f);	// Bottom Left Of The Texture and Quad
				glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*alus.jalkikoko*2, -1.0f*alus.jalkikoko*2,  1.0f);	// Bottom Right Of The Texture and Quad
				glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*alus.jalkikoko*2,  1.0f*alus.jalkikoko*2,  1.0f);	// Top Right Of The Texture and Quad
				glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*alus.jalkikoko*2,  1.0f*alus.jalkikoko*2,  1.0f);	// Top Left Of The Texture and Quad
			glEnd();

			glPopMatrix();
		}
	}
	if(alus.numero==4 || alus.numero==23){
		for(i=0;i<kaasumaara;i++){
			if(kaasujalki2[i].alpha>0.0f){
			glPushMatrix();
				glTranslatef(kaasujalki2[i].x,kaasujalki2[i].y,alustaso);
				glBindTexture(GL_TEXTURE_2D, texture[18]);
				glColor4f(1.0f,0.0f,0.0f,kaasujalki2[i].alpha);		
		
				glBegin(GL_QUADS);
					glNormal3f(0,0,1.0f);
					glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*alus.jalkikoko*2, -1.0f*alus.jalkikoko*2,  1.0f);	// Bottom Left Of The Texture and Quad
					glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*alus.jalkikoko*2, -1.0f*alus.jalkikoko*2,  1.0f);	// Bottom Right Of The Texture and Quad
					glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*alus.jalkikoko*2,  1.0f*alus.jalkikoko*2,  1.0f);	// Top Right Of The Texture and Quad
					glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*alus.jalkikoko*2,  1.0f*alus.jalkikoko*2,  1.0f);	// Top Left Of The Texture and Quad
				glEnd();		

				glPopMatrix();
			}
		}
	}
	glBlendFunc(GL_SRC_COLOR,GL_ONE_MINUS_SRC_COLOR);
}
void Player::piirraPadit(){
	int i;
	glDisable(GL_LIGHTING);
	glDisable(GL_DEPTH_TEST);
	glEnable(GL_TEXTURE_2D);
	glEnable(GL_BLEND);
	glBlendFunc(GL_ONE,GL_ONE);	
	if(alus.aselaji==1)glBlendFunc(GL_SRC_ALPHA,GL_ONE_MINUS_SRC_COLOR);
	else glBlendFunc(GL_SRC_COLOR,GL_ONE_MINUS_SRC_COLOR);
	if(alus.aselaji==13){
		glDisable(GL_TEXTURE_2D);
		glEnable(GL_LIGHTING);
		glBlendFunc(GL_SRC_ALPHA, GL_ONE);
		for(i=0;i<alus.maxpadit;i++) if(pati[i].aika>0){
			glPushMatrix();
			glTranslatef(pati[i].x,pati[i].y,alustaso);
			glScalef(2,2,2);
			//glRotatef(pati[i].kulma-90.0f,0.0f,0.0f,1.0f);						
			int aika = pati[i].energia;
			double alpha = 1 - exp(-aika*0.002);
			glColor4f(pati[i].r, pati[i].g, pati[i].b, alpha);
			int j;
			for (j = 1; j <= 10; j+=2){
				glRotatef(aika*(j+5), 0, 10-j, j);
				GLUquadricObj *gah=gluNewQuadric();
				gluSphere(gah, j*0.1, 10, 10);
				gluDeleteQuadric(gah);
			}
			glPopMatrix();
		}
		glDisable(GL_LIGHTING);
		glDisable(GL_DEPTH_TEST);
		glEnable(GL_TEXTURE_2D);
	}
	else if(alus.aselaji==4){
		for(i=0;i<alus.maxpadit;i++) if(pati[i].aika>0){
			glPushMatrix();
			glTranslatef(pati[i].x,pati[i].y,alustaso);
			glRotatef(alfa,0,0,1);
			glRotatef(beta,0,1,0);
			if(i==0){
				glColor4f(1.0f,1.0f,1.0f,1.0f);			
				glBindTexture(GL_TEXTURE_2D, alus.texture2);
				glBegin(GL_QUADS);
					glNormal3f(0,0,1.0f);
					glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*alus.epatisade, -1.0f*alus.epatisade,  1.0f);	// Bottom Left Of The Texture and Quad
					glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*alus.epatisade, -1.0f*alus.epatisade,  1.0f);	// Bottom Right Of The Texture and Quad
					glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*alus.epatisade,  1.0f*alus.epatisade,  1.0f);	// Top Right Of The Texture and Quad
					glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*alus.epatisade,  1.0f*alus.epatisade,  1.0f);	// Top Left Of The Texture and Quad
				glEnd();
			}else{
				glColor4f(1.0f,1.0f,1.0f,1.0f);			
				glBindTexture(GL_TEXTURE_2D, alus.patitexture);
				glBegin(GL_QUADS);
					glNormal3f(0,0,1.0f);
					glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*alus.patisade, -1.0f*alus.patisade,  1.0f);	// Bottom Left Of The Texture and Quad
					glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*alus.patisade, -1.0f*alus.patisade,  1.0f);	// Bottom Right Of The Texture and Quad
					glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*alus.patisade,  1.0f*alus.patisade,  1.0f);	// Top Right Of The Texture and Quad
					glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*alus.patisade,  1.0f*alus.patisade,  1.0f);	// Top Left Of The Texture and Quad
				glEnd();
			}
			glPopMatrix();
		}
	}else if(alus.aselaji==15){
		GLUquadricObj *temp;
		temp=gluNewQuadric();
		glEnable(GL_LIGHTING);
		glEnable(GL_DEPTH_TEST);
		glDisable(GL_TEXTURE_2D);
		glEnable(GL_BLEND);
		glBlendFunc(GL_SRC_ALPHA,GL_ONE_MINUS_SRC_ALPHA);	
		glColor4f(0.2f,0.2f,0.8f,0.5f);			
		for(i=0;i<alus.maxpadit;i++) if(pati[i].aika>0){
			glPushMatrix();
			glTranslatef(pati[i].x,pati[i].y,alustaso);
//			glRotatef(pati[i].kulma-90.0f,0.0f,0.0f,1.0f);						
			gluSphere(temp, 1.3, 10, 5);
/*			glBegin(GL_QUADS);
				glNormal3f(0,0,1.0f);
				glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*alus.patisade, -1.0f*alus.patisade,  1.0f);	// Bottom Left Of The Texture and Quad
				glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*alus.patisade, -1.0f*alus.patisade,  1.0f);	// Bottom Right Of The Texture and Quad
				glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*alus.patisade,  1.0f*alus.patisade,  1.0f);	// Top Right Of The Texture and Quad
				glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*alus.patisade,  1.0f*alus.patisade,  1.0f);	// Top Left Of The Texture and Quad
			glEnd();*/
			glPopMatrix();
		}
		gluDeleteQuadric(temp);
	}else{
		glBindTexture(GL_TEXTURE_2D, alus.patitexture);
		for(i=0;i<alus.maxpadit;i++) if(pati[i].aika>0){
			glPushMatrix();
			glTranslatef(pati[i].x,pati[i].y,alustaso);
			if(sets->cammode==0){
				glRotatef(alfa,0,0,1);
				glRotatef(beta,0,1,0);
				glRotatef(-1*alfa,0,0,1);
			}
			glRotatef(pati[i].kulma-90.0f,0.0f,0.0f,1.0f);						
			if(alus.numero==19){
				glColor4f(0.9f,0.9f,0.9f,2.0f);			
			}else if(alus.numero==21){
				glColor4f(0.5f,0.5f,1.0f,5.0f);			
			}else glColor4f(1.0f,1.0f,1.0f,1.0f);			
			glBegin(GL_QUADS);
				glNormal3f(0,0,1.0f);
				glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*alus.patisade, -1.0f*alus.patisade,  1.0f);	// Bottom Left Of The Texture and Quad
				glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*alus.patisade, -1.0f*alus.patisade,  1.0f);	// Bottom Right Of The Texture and Quad
				glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*alus.patisade,  1.0f*alus.patisade,  1.0f);	// Top Right Of The Texture and Quad
				glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*alus.patisade,  1.0f*alus.patisade,  1.0f);	// Top Left Of The Texture and Quad
			glEnd();
			glPopMatrix();
		}
	}

//	if(alus.aselaji==1)	glBlendFunc(GL_SRC_COLOR,GL_ONE_MINUS_SRC_COLOR);

}

void Player::piirraePadit(){
	glEnable(GL_BLEND);
	glBlendFunc(GL_ONE,GL_ONE);	
	int i;
	if(alus.numero==16){
//		glBindTexture(GL_TEXTURE_2D, alus.epatitexture);
		for(i=0;i<alus.maxepadit;i++) if(epati[i].aika>0){
			glPushMatrix();
			glTranslatef(epati[i].x,epati[i].y,alustaso);
			glColor4f(epati[i].r,epati[i].g,epati[i].b,1.0f);			
			glBegin(GL_QUADS);
				glNormal3f(0,0,1.0f);
				glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*alus.epatisade, -1.0f*alus.epatisade,  1.0f);
				glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*alus.epatisade, -1.0f*alus.epatisade,  1.0f);
				glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*alus.epatisade,  1.0f*alus.epatisade,  1.0f);
				glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*alus.epatisade,  1.0f*alus.epatisade,  1.0f);
			glEnd();
/*			glBegin(GL_TRIANGLES);
				glVertex3f(-1.0f*alus.epatisade, -1.0f*alus.epatisade,  0.0f);
				glVertex3f(-1.0f*alus.epatisade,  1.0f*alus.epatisade,  0.0f);
				glVertex3f( 1.0f*alus.epatisade,  0.0f*alus.epatisade,  0.0f);
			glEnd();*/

	
			glPopMatrix();
		}
	}else if(alus.numero==21){
		glBindTexture(GL_TEXTURE_2D, alus.epatitexture);
		for(i=0;i<alus.maxepadit;i++) if(epati[i].aika>0){
			glPushMatrix();
			glTranslatef(epati[i].x,epati[i].y,alustaso);
			double epatikoko;
			if(epati[i].r==1){
				glColor4f(1,0.7f,0.7f,1.0f);
				epatikoko=alus.epatisade*2;
			}else {
				glColor4f(0.7f,1.0f,0.7f,alpha);		
				epatikoko=alus.epatisade;
			}
			glBegin(GL_QUADS);
				glNormal3f(0,0,1.0f);
				glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*epatikoko, -1.0f*epatikoko,  1.0f);
				glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*epatikoko, -1.0f*epatikoko,  1.0f);
				glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*epatikoko,  1.0f*epatikoko,  1.0f);
				glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*epatikoko,  1.0f*epatikoko,  1.0f);
			glEnd();

	
			glPopMatrix();
		}
	}else{
		glBindTexture(GL_TEXTURE_2D, alus.epatitexture);
		for(i=0;i<alus.maxepadit;i++) if(epati[i].aika>0){
			glPushMatrix();
			glTranslatef(epati[i].x,epati[i].y,alustaso);
			glRotatef(epati[i].kulma-90.0f,0.0f,0.0f,1.0f);						
			if(alus.numero==13){
				glColor4f(0.1f,0.7f,0.1f,1.0f);
			}else glColor4f(1.0f,1.0f,1.0f,alpha);		
			glBegin(GL_QUADS);
				glNormal3f(0,0,1.0f);
				glTexCoord2f(0.0f, 1.0f); glVertex3f(-1.0f*alus.epatisade, -1.0f*alus.epatisade,  1.0f);
				glTexCoord2f(1.0f, 1.0f); glVertex3f( 1.0f*alus.epatisade, -1.0f*alus.epatisade,  1.0f);
				glTexCoord2f(1.0f, 0.0f); glVertex3f( 1.0f*alus.epatisade,  1.0f*alus.epatisade,  1.0f);
				glTexCoord2f(0.0f, 0.0f); glVertex3f(-1.0f*alus.epatisade,  1.0f*alus.epatisade,  1.0f);
			glEnd();

	
			glPopMatrix();
		}
	}
}


void Player::piirra(){
	alus.piirra(x,y,alustaso,kulma,asekulma,numero);
}

void Player::piirra(int x, int y, int z, int kulma, int asekulma){
	alus.piirra(x,y,z,kulma,asekulma,numero);
}

void Player::ammu(){
	double paikkoja;
	double ero,count;
	int i, j;
	double tarkistuskerrat,kulma2;
	double etaisyys,temp;
	switch(alus.numero){
	case 0:
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
		if(!joinpeli) for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika==0){
				if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
				pati[i].aika=alus.patiaika;
				pati[i].x=x+cos((kulma+45.0f)/180*PI)*alus.sade*0.9f;
				pati[i].y=y+sin((kulma+45.0f)/180*PI)*alus.sade*0.9f;
				pati[i].xv=cos(kulma/180*PI)*alus.patinopeus+xvauhti;
				pati[i].yv=sin(kulma/180*PI)*alus.patinopeus+yvauhti;
				pati[i].kulma=kulma;
				pati[i].energia=alus.patienergia;
				i=alus.maxpadit;

			}
		}
		if(!joinpeli) for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika==0){
				pati[i].aika=alus.patiaika;
				pati[i].x=x+cos((kulma-45.0f)/180*PI)*alus.sade*0.9f;
				pati[i].y=y+sin((kulma-45.0f)/180*PI)*alus.sade*0.9f;
				pati[i].xv=cos(kulma/180*PI)*alus.patinopeus+xvauhti;
				pati[i].yv=sin(kulma/180*PI)*alus.patinopeus+yvauhti;
				pati[i].kulma=kulma;
				pati[i].energia=alus.patienergia;
				i=alus.maxpadit;
			}
		}
		break;
	case 1:
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
		if(!joinpeli) for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika==0){
				pati[i].aika=alus.patiaika;
				if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
				pati[i].x=x+cos(kulma/180*PI)*alus.sade*0.6f;
				pati[i].y=y+sin(kulma/180*PI)*alus.sade*0.6f;
				pati[i].xv=cos(kulma/180*PI)*alus.patinopeus+xvauhti;
				pati[i].yv=sin(kulma/180*PI)*alus.patinopeus+yvauhti;
				pati[i].kulma=kulma;
				pati[i].energia=alus.patienergia;
				i=alus.maxpadit;

			}
		}
		xvauhti-=0.7f*cos(kulma/180*PI);
		yvauhti-=0.7f*sin(kulma/180*PI);
		break;
	case 2:
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
		if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
		if(!joinpeli) for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika==0){
				pati[i].aika=alus.patiaika;
				pati[i].x=x+cos(kulma/180*PI)*alus.sade*0.6f;
				pati[i].y=y+sin(kulma/180*PI)*alus.sade*0.6f;
				pati[i].xv=cos(kulma/180*PI)*alus.patinopeus+xvauhti;
				pati[i].yv=sin(kulma/180*PI)*alus.patinopeus+yvauhti;
				pati[i].kulma=kulma;
				pati[i].energia=alus.patienergia;
				i=alus.maxpadit;

			}
		}
		break;
	case 3:
		if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
		if(!joinpeli) for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika<=0){
				pati[i].aika=alus.patiaika;
				pati[i].x=x+cos((kulma+55.0f*(double)ampumispuoli)/180*PI)*alus.sade*0.8f;
				pati[i].y=y+sin((kulma+55.0f*(double)ampumispuoli)/180*PI)*alus.sade*0.8f;
				pati[i].xv=cos(kulma/180*PI)*alus.patinopeus+xvauhti;
				pati[i].yv=sin(kulma/180*PI)*alus.patinopeus+yvauhti;
				pati[i].kulma=kulma;
				pati[i].energia=alus.patienergia;
				ampumispuoli=-1*ampumispuoli;
				i=alus.maxpadit;
			}
		}
		break;



	case 4:
		i=0;
		if(!joinpeli) if(pati[i].aika<=0){
			if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
			ampumisviive=alus.ampumisnopeus;
			fuel-=alus.patifuel;
			pati[i].aika=alus.patiaika;
			pati[i].x=x+cos((kulma)/180*PI)*alus.sade*0.7f;
			pati[i].y=y+sin((kulma)/180*PI)*alus.sade*0.7f;
			pati[i].xv=cos(kulma/180*PI)*alus.patinopeus+xvauhti;
			pati[i].yv=sin(kulma/180*PI)*alus.patinopeus+yvauhti;
			pati[i].energia=alus.patienergia;
			pati[i].kulma=kulma;
		}
		break;
	case 5:
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
		for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika<=0){
				if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
				if(!joinpeli){
					pati[i].aika=alus.patiaika;
					pati[i].x=x+cos(kulma/180*PI)*alus.sade*0.6f;
					pati[i].y=y+sin(kulma/180*PI)*alus.sade*0.6f;
					pati[i].xv=cos(kulma/180*PI)*alus.patinopeus+xvauhti;
					pati[i].yv=sin(kulma/180*PI)*alus.patinopeus+yvauhti;
					pati[i].kulma=kulma;
					pati[i].energia=alus.patienergia;
				}
				i=alus.maxpadit;

			}
		}
		xvauhti-=0.1f*cos(kulma/180*PI);
		yvauhti-=0.1f*sin(kulma/180*PI);
		break;
	case 6:
		if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
		etaisyys=alus.patisade+10;
		for(i=0;i<players;i++) if(i!=numero && player[i].tiimi!=tiimi) if((!pistepeli) || player[i].pisteet>=0){
			double xet=player[i].x-x;
			double yet=player[i].y-y;
			double etaisyys2=xet*xet+yet*yet;
			if(etaisyys2<pow(alus.patisade+player[i].alus.sade*0.785,2)){
				if(!joinpeli) if(!(player[i].alus.easelaji==0 && player[i].suojapaalla))player[i].energia-=alus.damage;
//				if(player[i].energia<0) player[i].energia=0;
				viivat[0].x1=player[i].x;
				viivat[0].y1=player[i].y;
				viivat[0].x2=x;
				viivat[0].y2=y;
				viivat[0].aika=alus.patiaika;
				viivat[0].r=0;
				viivat[0].g=1;
				viivat[0].b=1;
				viivat[0].xv=0;
				viivat[0].yv=0;
				asekulma=atan2(yet,xet)*180.0f/PI;
				etaisyys=sqrt(etaisyys2);
				break;
			}
		}
		if(etaisyys>=alus.patisade+10) for(i=0;i<players;i++) if(i!=numero && player[i].tiimi!=tiimi) if((!pistepeli) || player[i].pisteet>=0){
			if(player[i].alus.patiVoiHajota){
				for(j=0;j<player[i].alus.maxpadit;j++){
					if(player[i].pati[j].aika>0){
						double xet=player[i].pati[j].x-x;
						double yet=player[i].pati[j].y-y;
						double etaisyys2=xet*xet+yet*yet;
						if(etaisyys2<pow(alus.patisade+player[i].alus.patisade*0.785,2)){
							if(!joinpeli) player[i].pati[j].energia-=alus.damage;
							if(!joinpeli) if(player[i].pati[j].energia<=0){
								player[i].pati[j].energia=0;
								player[i].pati[j].aika=0;
							}
							viivat[0].x1=player[i].pati[j].x;
							viivat[0].y1=player[i].pati[j].y;
							viivat[0].x2=x;
							viivat[0].y2=y;
							viivat[0].aika=alus.patiaika;
							viivat[0].r=0;
							viivat[0].g=1;
							viivat[0].b=1;
							viivat[0].xv=0;
							viivat[0].yv=0;
							asekulma=atan2(yet,xet)*180.0f/PI;
							//j=player[i].alus.maxpadit;
							i=players;
							etaisyys=sqrt(etaisyys2);
							break;
						}
					}
				}
			}
		}
		if(etaisyys>=alus.patisade+10) for(i=0;i<players;i++) if(i!=numero && player[i].tiimi!=tiimi) if((!pistepeli) || player[i].pisteet>=0){
			if(player[i].alus.epatiVoiHajota){
				for(j=0;j<player[i].alus.maxepadit;j++){
					if(player[i].epati[j].aika>0){
						double xet=player[i].epati[j].x-x;
						double yet=player[i].epati[j].y-y;
						double etaisyys2=xet*xet+yet*yet;
						if(etaisyys2<pow(alus.patisade+player[i].alus.epatisade*0.785,2)){
							if(!joinpeli) player[i].epati[j].energia-=alus.damage;
							if(!joinpeli) if(player[i].epati[j].energia<0){
								player[i].epati[j].energia=0;
								player[i].epati[j].aika=0;
							}
							viivat[0].x1=player[i].epati[j].x;
							viivat[0].y1=player[i].epati[j].y;
							viivat[0].x2=x;
							viivat[0].y2=y;
							viivat[0].aika=alus.patiaika;
							viivat[0].r=0;
							viivat[0].g=1;
							viivat[0].b=1;
							viivat[0].xv=0;
							viivat[0].yv=0;
							asekulma=atan2(yet,xet)*180.0f/PI;
							//j=player[i].alus.maxepadit;
							i=players;
							etaisyys=sqrt(etaisyys2);
							break;
						}
					}
				}
			}
		}if(etaisyys>=alus.patisade+10){
			viivat[0].x1=x+cos(kulma/180.0f*PI)*(alus.patisade+1);
			viivat[0].y1=y+sin(kulma/180.0f*PI)*(alus.patisade+1);
			viivat[0].x2=x;
			viivat[0].y2=y;
			viivat[0].aika=alus.patiaika;
			viivat[0].r=0;
			viivat[0].g=1;
			viivat[0].b=1;
			viivat[0].xv=0;
			viivat[0].yv=0;
			asekulma=kulma;
		}
		if(pylos){
//			viivat[0].x1=cos(kulma/180.0f*PI)*alus.nopeus;
//			viivat[0].y1=cos(kulma/180.0f*PI)*alus.nopeus;
			viivat[0].x2+=cos(kulma/180.0f*PI)*alus.nopeus;
			viivat[0].y2+=sin(kulma/180.0f*PI)*alus.nopeus;
		}
		break;
	case 7:
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
		for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika==0){
				if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
				if(!joinpeli){
					pati[i].aika=alus.patiaika;
					pati[i].x=x+cos(kulma/180*PI)*alus.sade*1.4f;
					pati[i].y=y+sin(kulma/180*PI)*alus.sade*1.4f;
					pati[i].xv=cos(kulma/180*PI)*alus.nopeus*150.0f+xvauhti;
					pati[i].yv=sin(kulma/180*PI)*alus.nopeus*150.0f+yvauhti;
					pati[i].kulma=kulma;
					pati[i].energia=alus.patienergia;
				}
				i=alus.maxpadit;
			}
		}
		break;
	case 8:
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
		temp=random(10000)/250.0f-20.0f;
		for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika==0){
				if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
				if(!joinpeli){
					pati[i].aika=alus.patiaika;
					pati[i].x=x+cos(((double)kulma+temp)/180.0f*PI)*alus.sade*0.8f;
					pati[i].y=y+sin(((double)kulma+temp)/180.0f*PI)*alus.sade*0.8f;
					pati[i].xv=cos(((double)kulma+temp)/180.0f*PI)*alus.patinopeus+xvauhti;
					pati[i].yv=sin(((double)kulma+temp)/180.0f*PI)*alus.patinopeus+yvauhti;
					pati[i].kulma=kulma;
					pati[i].energia=alus.epatienergia;
				}
				i=alus.maxpadit;
			}
		}
		break;
	case 9:
		if(efektitpaalla && !laseraani && ampumisaani!=NULL){
			ampumisaani->play();
			laseraani=true;
		}
		if(!joinpeli) fuel-=alus.patifuel;
		viivat[0].x1=x+alus.sade*1.6*cos(kulma/180.0f*PI)+xvauhti;
		viivat[0].y1=y+alus.sade*1.6*sin(kulma/180.0f*PI)+yvauhti;
		viivat[0].x2=x+alus.patisade*cos(kulma/180.0f*PI)+xvauhti;
		viivat[0].y2=y+alus.patisade*sin(kulma/180.0f*PI)+yvauhti;
		viivat[0].aika=alus.patiaika;
		viivat[0].r=0;
		viivat[0].g=1;
		viivat[0].b=0;
		viivat[0].xv=0;
		viivat[0].yv=0;
		int j,k;
		kulma2=atan2(viivat[0].y1-viivat[0].y2,viivat[0].x1-viivat[0].x2);
		tarkistuskerrat=10;
		if(!joinpeli) for(i=0;i<players;i++) if(i!=numero) if((!pistepeli) || player[i].pisteet>=0){
			for(j=0;j<tarkistuskerrat;j++){
				double xet=player[i].x-viivat[0].x1+cos(kulma2)*j/tarkistuskerrat*alus.patisade;
				double yet=player[i].y-viivat[0].y1+sin(kulma2)*j/tarkistuskerrat*alus.patisade;
				double etaisyys=xet*xet+yet*yet;
				if(etaisyys<pow(0.2f+player[i].alus.sade*0.785,2)){
					player[i].energia-=alus.damage;
					j=tarkistuskerrat;
				}
			}
			for(k=0;k<player[i].alus.maxpadit;k++) if(player[i].pati[k].aika>0){
				for(j=0;j<tarkistuskerrat;j++){
					double xet=player[i].pati[k].x-viivat[0].x1+cos(kulma2)*j/tarkistuskerrat*alus.patisade;
					double yet=player[i].pati[k].y-viivat[0].y1+sin(kulma2)*j/tarkistuskerrat*alus.patisade;
					double etaisyys=xet*xet+yet*yet;
					if(etaisyys<pow(0.2f+player[i].alus.patisade*0.785,2)){
						player[i].pati[k].energia-=alus.damage;
						if(!joinpeli)if(player[i].pati[k].energia<0){
							player[i].pati[k].energia=0;
							player[i].pati[k].aika=0;
						}
						j=tarkistuskerrat;
					}
				}
			}
			for(k=0;k<player[i].alus.maxepadit;k++) if(player[i].epati[k].aika>0){
				for(j=0;j<tarkistuskerrat;j++){
					double xet=player[i].epati[k].x-viivat[0].x1+cos(kulma2)*j/tarkistuskerrat*alus.patisade;
					double yet=player[i].epati[k].y-viivat[0].y1+sin(kulma2)*j/tarkistuskerrat*alus.patisade;
					double etaisyys=xet*xet+yet*yet;
					if(etaisyys<pow(0.2f+player[i].alus.epatisade*0.785,2)){
						player[i].epati[k].energia-=alus.damage;
						if(!joinpeli)if(player[i].epati[k].energia<0){
							player[i].epati[k].energia=0;
							player[i].epati[k].aika=0;
						}
						j=tarkistuskerrat;
					}
				}
			}
		}
		break;
	case 10:
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
//		double temp=random(10000)/250.0f-20.0f;
		for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika==0){
				if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
				if(!joinpeli){
					pati[i].aika=alus.patiaika;
					pati[i].x=x+cos(((double)kulma)/180.0f*PI)*alus.sade*0.8f;
					pati[i].y=y+sin(((double)kulma)/180.0f*PI)*alus.sade*0.8f;
					pati[i].xv=cos(((double)kulma)/180.0f*PI)*alus.patinopeus+xvauhti;
					pati[i].yv=sin(((double)kulma)/180.0f*PI)*alus.patinopeus+yvauhti;
					pati[i].kulma=kulma;
					pati[i].energia=alus.epatienergia;
				}
				i=alus.maxpadit;
			}
		}
		break;
	case 11:
		if(suojapaalla){
			ampumisviive=alus.ampumisnopeus;
			if(!joinpeli) fuel-=alus.patifuel;
			for(i=0;i<alus.maxpadit;i++){
				if(pati[i].aika==0){
					if(efektitpaalla && eampumisaani!=NULL) eampumisaani->play();
					if(!joinpeli){
						pati[i].aika=alus.patiaika;
						pati[i].x=x+cos((kulma+15.0f)/180*PI)*alus.sade*1.6f;
						pati[i].y=y+sin((kulma+15.0f)/180*PI)*alus.sade*1.6f;
						pati[i].xv=cos(kulma/180*PI)*alus.patinopeus+xvauhti;
						pati[i].yv=sin(kulma/180*PI)*alus.patinopeus+yvauhti;
						pati[i].kulma=kulma;
						pati[i].energia=alus.epatienergia;
					}
					i=alus.maxpadit;

				}
			}
			if(!joinpeli) for(i=0;i<alus.maxpadit;i++){
				if(pati[i].aika==0){
					pati[i].aika=alus.patiaika;
					pati[i].x=x+cos((kulma-15.0f)/180*PI)*alus.sade*1.6f;
					pati[i].y=y+sin((kulma-15.0f)/180*PI)*alus.sade*1.6f;
					pati[i].xv=cos(kulma/180*PI)*alus.patinopeus+xvauhti;
					pati[i].yv=sin(kulma/180*PI)*alus.patinopeus+yvauhti;
					pati[i].kulma=kulma;
					pati[i].energia=alus.patienergia;
					i=alus.maxpadit;
				}
			}
		}else{
			ampumisviive=alus.ampumisnopeus;
			if(efektitpaalla){// && !laseraani){
				ampumisaani->play();
				laseraani=true;
			}
			if(!joinpeli) fuel-=alus.patifuel;///30.0;
			viivat[0].x1=x+alus.sade*1.0*cos(kulma/180.0f*PI)+xvauhti;
			viivat[0].y1=y+alus.sade*1.0*sin(kulma/180.0f*PI)+yvauhti;
			viivat[0].x2=x+((alus.epatisade-1)/4.0+1)*cos(kulma/180.0f*PI)+xvauhti;
			viivat[0].y2=y+((alus.epatisade-1)/4.0+1)*sin(kulma/180.0f*PI)+yvauhti;
			viivat[0].xv=cos(kulma/180.0f*PI)*(alus.epatisade-1)/6.0;
			viivat[0].yv=sin(kulma/180.0f*PI)*(alus.epatisade-1)/6.0;
			viivat[0].aika=6;
			viivat[0].r=0;
			viivat[0].g=0;
			viivat[0].b=1;
			int j,k;
			double kulma2=atan2(viivat[0].y1-viivat[0].y2,viivat[0].x1-viivat[0].x2);
			double tarkistuskerrat=10;
			if(!joinpeli) for(i=0;i<players;i++) if(i!=numero) if((!pistepeli) || player[i].pisteet>=0){
				for(j=0;j<tarkistuskerrat;j++){
					double xet=player[i].x-viivat[0].x1+cos(kulma2)*j/tarkistuskerrat*alus.epatisade;
					double yet=player[i].y-viivat[0].y1+sin(kulma2)*j/tarkistuskerrat*alus.epatisade;
					double etaisyys=xet*xet+yet*yet;
					if(etaisyys<pow(0.2f+player[i].alus.sade*0.785,2)){
						player[i].energia-=alus.edamage;
						j=tarkistuskerrat;
					}
				}
				for(k=0;k<player[i].alus.maxpadit;k++) if(player[i].pati[k].aika>0){
					for(j=0;j<tarkistuskerrat;j++){
						double xet=player[i].pati[k].x-viivat[0].x1+cos(kulma2)*j/tarkistuskerrat*alus.epatisade;
						double yet=player[i].pati[k].y-viivat[0].y1+sin(kulma2)*j/tarkistuskerrat*alus.epatisade;
						double etaisyys=xet*xet+yet*yet;
						if(etaisyys<pow(0.2f+player[i].alus.patisade*0.785,2)){
							player[i].pati[k].energia-=alus.edamage;
							if(player[i].pati[k].energia<=0){
								player[i].pati[k].energia=0;
								player[i].pati[k].aika=0;
							}
							j=tarkistuskerrat;
						}
					}
				}
				for(k=0;k<player[i].alus.maxepadit;k++) if(player[i].epati[k].aika>0){
					for(j=0;j<tarkistuskerrat;j++){
						double xet=player[i].epati[k].x-viivat[0].x1+cos(kulma2)*j/tarkistuskerrat*alus.epatisade;
						double yet=player[i].epati[k].y-viivat[0].y1+sin(kulma2)*j/tarkistuskerrat*alus.epatisade;
						double etaisyys=xet*xet+yet*yet;
						if(etaisyys<pow(0.2f+player[i].alus.epatisade*0.785,2)){
							player[i].epati[k].energia-=alus.edamage;
							if(player[i].epati[k].energia<0){
								player[i].epati[k].energia=0;
								player[i].epati[k].aika=0;
							}
							j=tarkistuskerrat;
						}
					}
				}
			}
		}
		break;
	case 12:
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
//		int j,kulma2;
		kulma2=-90;
		for(j=0;j<3;j++){
			for(i=0;i<alus.maxpadit;i++){
				if(pati[i].aika==0){
					if(efektitpaalla) ampumisaani->play();
					if(!joinpeli){
						pati[i].aika=alus.patiaika;
						pati[i].x=x+cos((kulma+kulma2)/180.0f*PI)*alus.sade*0.8f;
						pati[i].y=y+sin((kulma+kulma2)/180.0f*PI)*alus.sade*0.8f;
						pati[i].xv=cos((kulma+kulma2)/180.0f*PI)*alus.patinopeus+xvauhti;
						pati[i].yv=sin((kulma+kulma2)/180.0f*PI)*alus.patinopeus+yvauhti;
						pati[i].kulma=kulma;
						pati[i].energia=alus.patienergia;
					}
					i=alus.maxpadit;

				}
			}
			kulma2+=90;
		}
		break;
	case 13:
		int i;
		if((pfire && !joinpeli) || (joinase && joinpeli)){
			if(pati[0].aika<=0 && !joinpeli){
				pati[0].aika=alus.patiaika;
				pati[0].x=x+cos(kulma/180.0f*PI)*alus.sade*1.5f;
				pati[0].y=y+sin(kulma/180.0f*PI)*alus.sade*1.5f;
				pati[0].energia=100;
				pati[0].xv=0;
				pati[0].yv=0;
				pati[0].r=0;
				pati[0].g=1.0f;
				pati[0].b=0;
			}else if(pati[0].aika>0){
				pati[0].aika=alus.patiaika;
				if(pati[0].energia<2000) pati[0].energia++;
				pati[0].x=x+cos(kulma/180.0f*PI)*alus.sade*1.5f;
				pati[0].y=y+sin(kulma/180.0f*PI)*alus.sade*1.5f;
				pati[0].xv=0;
				pati[0].yv=0;
				if(pati[0].r<1)pati[0].r+=0.001;
				if(pati[0].g>0)pati[0].g-=0.001;
			}
		}else if(!joinpeli || (joinpeli && !joinase)){
			if(pati[0].aika>0) if(efektitpaalla) ampumisaani->play();
			if(!joinpeli) for(i=1;i<alus.maxpadit;i++) if(pati[i].aika<=0){
				ampumisviive=alus.ampumisnopeus;
				pati[i].aika=alus.patiaika;
				pati[i].x=x+cos(kulma/180.0f*PI)*alus.sade*1.5f;
				pati[i].y=y+sin(kulma/180.0f*PI)*alus.sade*1.5f;
				pati[i].xv=cos(kulma/180.0f*PI)*alus.patinopeus+xvauhti;
				pati[i].yv=sin(kulma/180.0f*PI)*alus.patinopeus+yvauhti;
				pati[i].energia=pati[0].energia;
				pati[i].r=pati[0].r;
				pati[i].g=pati[0].g;
				pati[i].b=pati[0].b;
				if(!joinpeli) pati[0].aika=0;
				i=alus.maxpadit;
			}
		}
		break;
	case 14:
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
		for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika<=0){
				if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
				if(!joinpeli){
					pati[i].aika=alus.patiaika;
					pati[i].x=x+cos(kulma/180*PI)*alus.sade*1.4f;
					pati[i].y=y+sin(kulma/180*PI)*alus.sade*1.4f;
					pati[i].xv=cos(kulma/180*PI)*alus.nopeus*150.0f+xvauhti;
					pati[i].yv=sin(kulma/180*PI)*alus.nopeus*150.0f+yvauhti;
					pati[i].kulma=kulma;
					pati[i].energia=alus.patienergia;
				}
				i=alus.maxpadit;

			
			}
		}
		break;
	case 15:
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
		for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika<=0){
				if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
				if(!joinpeli){
					pati[i].aika=alus.patiaika;
					pati[i].x=random(800)/400.0f-1+x;
					pati[i].y=random(800)/400.0f-1+y;
					pati[i].xv=0;
					pati[i].yv=0;
					pati[i].kulma=kulma;
					pati[i].energia=alus.patienergia;
				}
				i=alus.maxpadit;
			}
		}
		break;
	case 16:
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
		for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika<=0){
				if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
				if(!joinpeli){
					pati[i].aika=alus.patiaika;
					pati[i].x=x+cos((kulma+26.0f*ampumispuoli)/180.0f*PI)*alus.sade*1.5f;
					pati[i].y=y+sin((kulma+26.0f*ampumispuoli)/180.0f*PI)*alus.sade*1.5f;
					pati[i].xv=cos(kulma/180.0f*PI)*alus.patinopeus+xvauhti;
					pati[i].yv=sin(kulma/180.0f*PI)*alus.patinopeus+yvauhti;
					pati[i].kulma=kulma;
					pati[i].energia=alus.patienergia;
				}
				i=alus.maxpadit;
				ampumispuoli*=-1;
			}
		}
		break;
	case 17:
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
		for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika==0){
				if(efektitpaalla) ampumisaani->play();
				if(!joinpeli){
					pati[i].aika=alus.patiaika;
					pati[i].x=x+cos(kulma/180*PI)*alus.sade*0.6f;
					pati[i].y=y+sin(kulma/180*PI)*alus.sade*0.6f;
					pati[i].xv=cos(kulma/180*PI)*alus.patinopeus+xvauhti;
					pati[i].yv=sin(kulma/180*PI)*alus.patinopeus+yvauhti;
					pati[i].kulma=kulma;
					pati[i].energia=alus.patienergia;
				}
				i=alus.maxpadit;
			}
		}
		break;
	case 18:
		if(efektitpaalla) ampumisaani->play();
		ampumisviive=alus.ampumisnopeus;
		//fuel-=alus.patifuel;
		etaisyys=alus.patisade+10;
//		double viivax, viivay;
		for(i=0;i<players;i++) if(i!=numero && player[i].tiimi!=tiimi) if((!pistepeli) || player[i].pisteet>=0){
			double xet=player[i].x-x;
			double yet=player[i].y-y;
			double etaisyys2=xet*xet+yet*yet;
			if(etaisyys2<pow(alus.patisade+player[i].alus.sade*0.785,2)){
				if(!joinpeli) if(!(player[i].alus.easelaji==0 && player[i].suojapaalla))
					player[i].energia-=alus.damage*(random(50)/100.0f+0.5f);
//				if(player[i].energia<0) player[i].energia=0;
				viivat[0].x1=player[i].x;
				viivat[0].y1=player[i].y;
				viivat[0].x2=x;
				viivat[0].y2=y;
				viivat[0].aika=alus.patiaika;
				viivat[0].r=0;
				viivat[0].g=1;
				viivat[0].b=1;
				viivat[0].xv=0;
				viivat[0].yv=0;
				i=players;
				etaisyys=sqrt(etaisyys2);
			}
		}
		if(etaisyys>=alus.patisade+10) for(i=0;i<players;i++) if(i!=numero && player[i].tiimi!=tiimi) if((!pistepeli) || player[i].pisteet>=0){
			if(player[i].alus.patiVoiHajota){
				for(j=0;j<player[i].alus.maxpadit;j++){
					if(player[i].pati[j].aika>0){
						double xet=player[i].pati[j].x-x;
						double yet=player[i].pati[j].y-y;
						double etaisyys2=xet*xet+yet*yet;
						if(etaisyys2<pow(alus.patisade+player[i].alus.patisade*0.785,2)){
							if(!joinpeli) player[i].pati[j].energia-=alus.damage;
							if(!joinpeli) if(player[i].pati[j].energia<=0){
								player[i].pati[j].energia=0;
								player[i].pati[j].aika=0;
							}
							viivat[0].x1=player[i].pati[j].x;
							viivat[0].y1=player[i].pati[j].y;
							viivat[0].x2=x;
							viivat[0].y2=y;
							viivat[0].aika=alus.patiaika;
							viivat[0].r=0;
							viivat[0].g=1;
							viivat[0].b=1;
							viivat[0].xv=0;
							viivat[0].yv=0;
							//j=player[i].alus.maxpadit;
							i=players;
							etaisyys=sqrt(etaisyys2);
							break;
						}
					}
				}
			}
		}
		if(etaisyys>=alus.patisade+10) for(i=0;i<players;i++) if(i!=numero && player[i].tiimi!=tiimi) if((!pistepeli) || player[i].pisteet>=0){
			if(player[i].alus.epatiVoiHajota){
				for(j=0;j<player[i].alus.maxepadit;j++){
					if(player[i].epati[j].aika>0){
						double xet=player[i].epati[j].x-x;
						double yet=player[i].epati[j].y-y;
						double etaisyys2=xet*xet+yet*yet;
						if(etaisyys2<pow(alus.patisade+player[i].alus.epatisade*0.785,2)){
							if(!joinpeli) player[i].epati[j].energia-=alus.damage;
							if(!joinpeli) if(player[i].epati[j].energia<0){
								player[i].epati[j].energia=0;
								player[i].epati[j].aika=0;
							}
							viivat[0].x1=player[i].epati[j].x;
							viivat[0].y1=player[i].epati[j].y;
							viivat[0].x2=x;
							viivat[0].y2=y;
							viivat[0].aika=alus.patiaika;
							viivat[0].r=0;
							viivat[0].g=1;
							viivat[0].b=1;
							viivat[0].xv=0;
							viivat[0].yv=0;
							//j=player[i].alus.maxepadit;
							i=players;
							etaisyys=sqrt(etaisyys2);
							break;
						}
					}
				}
			}
		}
		if(etaisyys>=alus.patisade+10){
			viivat[0].x1=x+cos(kulma/180.0f*PI)*(alus.patisade+1);
			viivat[0].y1=y+sin(kulma/180.0f*PI)*(alus.patisade+1);
			viivat[0].x2=x;
			viivat[0].y2=y;
			viivat[0].aika=alus.patiaika;
			viivat[0].r=0;
			viivat[0].g=1;
			viivat[0].b=1;
			viivat[0].xv=0;
			viivat[0].yv=0;
			asekulma=kulma;
			fuel-=alus.patifuel/3;
		}else fuel-=(random(50)/100.0f+0.5f)*alus.patifuel;

/*		if(pylos){
			viivat[0].x2+=cos(kulma/180.0f*PI)*alus.nopeus;
			viivat[0].y2+=sin(kulma/180.0f*PI)*alus.nopeus;
		}*/
		//tehdään salama
		paikkoja=10;
		ero=2.5;
		viivat[0].aika=0;
		for(i=0;i<paikkoja;i++){
			viivat[i+1].x1=viivat[0].x2+i*(viivat[0].x1-viivat[0].x2)/(paikkoja);
			viivat[i+1].y1=viivat[0].y2+i*(viivat[0].y1-viivat[0].y2)/(paikkoja);
			viivat[i+1].x2=viivat[0].x2+(i+1)*(viivat[0].x1-viivat[0].x2)/(paikkoja);
			viivat[i+1].y2=viivat[0].y2+(i+1)*(viivat[0].y1-viivat[0].y2)/(paikkoja);
			viivat[i+1].aika=2;
			viivat[i+1].r=0.2;
			viivat[i+1].g=1.0;
			viivat[i+1].b=1.0;
			viivat[i+1].xv=0;
			viivat[i+1].yv=0;
			//toinen salama
			viivat[i+1+(int)paikkoja].x1=viivat[0].x2+i*(viivat[0].x1-viivat[0].x2)/(paikkoja);
			viivat[i+1+(int)paikkoja].y1=viivat[0].y2+i*(viivat[0].y1-viivat[0].y2)/(paikkoja);
			viivat[i+1+(int)paikkoja].x2=viivat[0].x2+(i+1)*(viivat[0].x1-viivat[0].x2)/(paikkoja);
			viivat[i+1+(int)paikkoja].y2=viivat[0].y2+(i+1)*(viivat[0].y1-viivat[0].y2)/(paikkoja);
			viivat[i+1+(int)paikkoja].aika=2;
			viivat[i+1+(int)paikkoja].r=0.2;
			viivat[i+1+(int)paikkoja].g=1.0;
			viivat[i+1+(int)paikkoja].b=1.0;
			viivat[i+1+(int)paikkoja].xv=0;
			viivat[i+1+(int)paikkoja].yv=0;

		
		}
		for(i=1;i<paikkoja;i++){
			double xheitto=random(100)/100.0f*ero;
			double yheitto=random(100)/100.0f*ero;
//			if(i<=paikkoja/2){
				xheitto*=i/(paikkoja/2);
				yheitto*=i/(paikkoja/2);
//			}else{
//				xheitto*=i/(paikkoja/2);
//				yheitto*=i/(paikkoja/2);
//			}
			viivat[i].x2=viivat[i].x2+xheitto;
			viivat[i+1].x1=viivat[i].x2;
			viivat[i].y2=viivat[i].y2+yheitto;
			viivat[i+1].y1=viivat[i].y2;

			//toinen salama
			xheitto=random(100)/100.0f*ero;
			yheitto=random(100)/100.0f*ero;
			if(i<=paikkoja/2){
				xheitto*=i/(paikkoja/2);
				yheitto*=i/(paikkoja/2);
			}else{
				xheitto*=i/(paikkoja/2);
				yheitto*=i/(paikkoja/2);
			}
			viivat[i+(int)paikkoja].x2=viivat[i+(int)paikkoja].x2+xheitto;
			viivat[i+1+(int)paikkoja].x1=viivat[i+(int)paikkoja].x2;
			viivat[i+(int)paikkoja].y2=viivat[i+(int)paikkoja].y2+yheitto;
			viivat[i+1+(int)paikkoja].y1=viivat[i+(int)paikkoja].y2;

		}

		break;
	case 19:
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
		if(ampumisjuttu<100) ampumisjuttu+=7;
		for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika<=0){
				if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
				if(!joinpeli){
					pati[i].aika=alus.patiaika-random(ampumisjuttu);
					pati[i].x=x+cos((kulma+26.0f*1)/180.0f*PI)*alus.sade*1.5f;
					pati[i].y=y+sin((kulma+26.0f*1)/180.0f*PI)*alus.sade*1.5f;
					pati[i].xv=cos(kulma/180.0f*PI)*alus.patinopeus*(random(ampumisjuttu)/100.0f+1-ampumisjuttu/100.0f)+xvauhti;
					pati[i].yv=sin(kulma/180.0f*PI)*alus.patinopeus*(random(ampumisjuttu)/100.0f+1-ampumisjuttu/100.0f)+yvauhti;
					pati[i].kulma=kulma;
					pati[i].energia=alus.patienergia/(random(ampumisjuttu)/100.0f+1);
				}
				i++;
//				i=alus.maxpadit;
//				ampumispuoli*=-1;
				break;
			}
		}
		for(;i<alus.maxpadit;i++){
			if(pati[i].aika<=0){
				if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
				if(!joinpeli){
					pati[i].aika=alus.patiaika-random(ampumisjuttu);
					pati[i].x=x+cos((kulma+26.0f*-1)/180.0f*PI)*alus.sade*1.5f;
					pati[i].y=y+sin((kulma+26.0f*-1)/180.0f*PI)*alus.sade*1.5f;
					pati[i].xv=cos(kulma/180.0f*PI)*alus.patinopeus*(random(ampumisjuttu)/100.0f+1-ampumisjuttu/100.0f)+xvauhti;
					pati[i].yv=sin(kulma/180.0f*PI)*alus.patinopeus*(random(ampumisjuttu)/100.0f+1-ampumisjuttu/100.0f)+yvauhti;
					pati[i].kulma=kulma;
					pati[i].energia=alus.patienergia/(random(ampumisjuttu)/100.0f+1);
				}
				break;
			}
		}
		break;
	case 20:
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
		for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika<=0){
				if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
				if(!joinpeli){
					pati[i].aika=alus.patiaika;
					pati[i].x=x+cos((kulma)/180.0f*PI)*alus.sade*1.6f;
					pati[i].y=y+sin((kulma)/180.0f*PI)*alus.sade*1.6f;
					pati[i].xv=cos(kulma/180.0f*PI)*alus.patinopeus+xvauhti;
					pati[i].yv=sin(kulma/180.0f*PI)*alus.patinopeus+yvauhti;
					pati[i].kulma=kulma;
					pati[i].energia=alus.patienergia;
				}
				break;
			}
		}
		break;
	case 21:
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
		count=18;
		ero=18;
		if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
		if(!joinpeli) for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika<=0){
				pati[i].aika=alus.patiaika;
				pati[i].x=x+cos((kulma+60.0-count*120.0/ero)/180.0f*PI)*alus.sade*3.5f;
				pati[i].y=y+sin((kulma+60.0-count*120.0/ero)/180.0f*PI)*alus.sade*3.5f;
				pati[i].xv=cos((kulma+45.0-count*90.0/ero)/180.0f*PI)*alus.patinopeus*(sin(count*PI/ero)+0.3)+xvauhti;
				pati[i].yv=sin((kulma+45.0-count*90.0/ero)/180.0f*PI)*alus.patinopeus*(sin(count*PI/ero)+0.3)+yvauhti;
				pati[i].kulma=kulma;
				pati[i].energia=alus.patienergia;
				count--;
				if(count<=0) break;
			}
		}
		break;
	case 22:
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
		for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika<=0){
				if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
				if(!joinpeli){
					pati[i].aika=alus.patiaika;
					pati[i].x=x+cos((kulma)/180.0f*PI)*alus.sade*2.5f;
					pati[i].y=y+sin((kulma)/180.0f*PI)*alus.sade*2.5f;
					pati[i].xv=cos((kulma)/180.0f*PI)*alus.patinopeus+xvauhti;
					pati[i].yv=sin((kulma)/180.0f*PI)*alus.patinopeus+yvauhti;
					pati[i].kulma=kulma;
					pati[i].energia=alus.patienergia;
				}
				break;
			}
		}
		break;
	case 23:
		ampumisviive=alus.ampumisnopeus;
		if(!joinpeli) fuel-=alus.patifuel;
		for(i=0;i<alus.maxpadit;i++){
			if(pati[i].aika<=0){
				if(efektitpaalla && ampumisaani!=NULL) ampumisaani->play();
				if(!joinpeli){
					pati[i].aika=alus.patiaika;
					pati[i].x=x+cos((kulma)/180.0f*PI)*alus.sade*1.5f;
					pati[i].y=y+sin((kulma)/180.0f*PI)*alus.sade*1.5f;
					pati[i].xv=cos((kulma)/180.0f*PI)*alus.patinopeus+xvauhti;
					pati[i].yv=sin((kulma)/180.0f*PI)*alus.patinopeus+yvauhti;
					pati[i].kulma=kulma;
					pati[i].energia=alus.patienergia;
					xvauhti-=pati[i].xv*alus.patipotkasu;
					yvauhti-=pati[i].yv*alus.patipotkasu;
				}
				break;
			}
		}
		break;
	default:
		break;
	}
}

void Player::tarkistaHajosikoPadit(){
	int i,j,k;
	for(j=numero;j<players;j++) if((!pistepeli) || player[j].pisteet>=0){

		 //pati vs pati
		if((alus.patiVoiHajota && player[j].alus.patihajottaa) || (player[j].alus.patiVoiHajota && alus.patihajottaa)){
			for(i=0;i<alus.maxpadit;i++){
				if(pati[i].aika>0){
					float sateet=player[j].alus.patisade+alus.patisade;
					for(k=0;k<player[j].alus.maxpadit;k++){
						if(player[j].pati[k].aika>0 && !(j==numero && k==i)){
							float xpatiet=player[j].pati[k].x-pati[i].x;
							float ypatiet=player[j].pati[k].y-pati[i].y;
							if(xpatiet<sateet && xpatiet>-sateet	//tarkistus neliötörmäyksille
								&& ypatiet<sateet && ypatiet>-sateet){
								if(xpatiet*xpatiet+ypatiet*ypatiet<pow(sateet*0.785,2)){ //tarkempi tarkistus
									if(player[j].alus.patiVoiHajota && alus.patihajottaa){
										player[j].pati[k].energia-=alus.damage;
										if(player[j].pati[k].energia<=0) player[j].pati[k].aika=0;
									}
									if(alus.patiVoiHajota && player[j].alus.patihajottaa){
										pati[i].energia-=player[j].alus.damage;
										if(pati[i].energia<=0) pati[i].aika=0;
										k=player[j].alus.maxpadit; //break
									}
								}
							}
						}

					}
				}
			}
		}

		//pati vs epati
		if((alus.patiVoiHajota && player[j].alus.epatihajottaa) || (player[j].alus.epatiVoiHajota && alus.patihajottaa)){
			for(i=0;i<alus.maxpadit;i++){
				if(pati[i].aika>0){
					float sateet=player[j].alus.epatisade+alus.patisade;
					for(k=0;k<player[j].alus.maxepadit;k++){
						if(player[j].epati[k].aika>0 && !(j==numero && k==i)){
							float xpatiet=player[j].epati[k].x-pati[i].x;
							float ypatiet=player[j].epati[k].y-pati[i].y;
							if(xpatiet<sateet && xpatiet>-sateet	//tarkistus neliötörmäyksille
								&& ypatiet<sateet && ypatiet>-sateet){
								if(xpatiet*xpatiet+ypatiet*ypatiet<pow(sateet*0.785,2)){ //tarkempi tarkistus
									if(player[j].alus.epatiVoiHajota && alus.patihajottaa){
										player[j].epati[k].energia-=alus.damage;
										if(player[j].epati[k].energia<=0) player[j].epati[k].aika=0;
									}
									if(alus.patiVoiHajota && player[j].alus.epatihajottaa){
										pati[i].energia-=player[j].alus.edamage;
										if(pati[i].energia<=0) pati[i].aika=0;
										k=player[j].alus.maxepadit; //break
									}
								}
							}
						}

					}
				}
			}
		}


		 //epati vs pati
		if((alus.epatiVoiHajota && player[j].alus.patihajottaa) || (player[j].alus.patiVoiHajota && alus.epatihajottaa)){
			for(i=0;i<alus.maxepadit;i++){
				if(epati[i].aika>0){
					float sateet=player[j].alus.patisade+alus.patisade;
					for(k=0;k<player[j].alus.maxpadit;k++){
						if(player[j].pati[k].aika>0 && !(j==numero && k==i)){
							float xpatiet=player[j].pati[k].x-epati[i].x;
							float ypatiet=player[j].pati[k].y-epati[i].y;
							if(xpatiet<sateet && xpatiet>-sateet	//tarkistus neliötörmäyksille
								&& ypatiet<sateet && ypatiet>-sateet){
								if(xpatiet*xpatiet+ypatiet*ypatiet<pow(sateet*0.785,2)){ //tarkempi tarkistus
									if(player[j].alus.patiVoiHajota && alus.epatihajottaa){
										player[j].pati[k].energia-=alus.edamage;
										if(player[j].pati[k].energia<=0) player[j].pati[k].aika=0;
									}
									if(alus.epatiVoiHajota && player[j].alus.patihajottaa){
										epati[i].energia-=player[j].alus.damage;
										if(epati[i].energia<=0) epati[i].aika=0;
										k=player[j].alus.maxpadit; //break
									}
								}
							}
						}

					}
				}
			}
		}


		//epati vs epati
		if((alus.epatiVoiHajota && player[j].alus.epatihajottaa) || (player[j].alus.epatiVoiHajota && alus.epatihajottaa)){
			if(  !(j==numero && alus.numero==17)
				&& !(player[j].alus.maxepadit>=180 && alus.maxepadit>=180)
				&& !(j==numero && alus.numero==22)
					){//määrittelyt omien patien törmäysestoiksi
				for(i=0;i<alus.maxepadit;i++){
					if(epati[i].aika>0){
						float sateet=player[j].alus.epatisade+alus.epatisade;
						for(k=0;k<player[j].alus.maxepadit;k++){
							if(player[j].epati[k].aika>0 && !(j==numero && k==i)){
								float xpatiet=player[j].epati[k].x-epati[i].x;
								float ypatiet=player[j].epati[k].y-epati[i].y;
								if(xpatiet<sateet && xpatiet>-sateet	//tarkistus neliötörmäyksille
									&& ypatiet<sateet && ypatiet>-sateet){
									if(xpatiet*xpatiet+ypatiet*ypatiet<pow(sateet*0.785,2)){ //tarkempi tarkistus
										if(player[j].alus.epatiVoiHajota && alus.epatihajottaa){
											player[j].epati[k].energia-=alus.edamage;
											if(player[j].epati[k].energia<=0) player[j].epati[k].aika=0;
										}
										if(alus.epatiVoiHajota && player[j].alus.epatihajottaa){
											epati[i].energia-=player[j].alus.edamage;
											if(epati[i].energia<=0) epati[i].aika=0;
											k=player[j].alus.maxepadit; //break
										}
									}
								}
							}
						}
					}
				}
			}
		}


	}
}

//void Player::tarkistaHajosikoPadit(){
/*void tempaskonen(){
	int i,j,k;
	if(alus.patiVoiHajota) for(i=0;i<alus.maxpadit;i++){
		if(pati[i].aika>0){
			for(j=0;j<players;j++) if((!pistepeli) || player[j].pisteet>=0){
				if(player[j].alus.patihajottaa && 
						(j>=numero || !player[j].alus.patiVoiHajota)){// || (player[j].alus.patiVoiHajota && alus.patihajottaa)){
					for(k=0;k<player[j].alus.maxpadit;k++) if(!(j==numero && k==i)) if(player[j].pati[k].aika>0){
						double xpatiet=player[j].pati[k].x-pati[i].x;
						double ypatiet=player[j].pati[k].y-pati[i].y;
						if(xpatiet<player[j].alus.patisade+alus.patisade
							&& ypatiet<player[j].alus.patisade+alus.patisade)
						if(xpatiet*xpatiet+ypatiet*ypatiet<
							pow((player[j].alus.patisade+alus.patisade)*0.785,2)){
							if(player[j].alus.patiVoiHajota){
								player[j].pati[k].energia-=alus.damage;
								if(player[j].pati[k].energia<=0) player[j].pati[k].aika=0;
							}
							pati[i].energia-=player[j].alus.damage;
							if(pati[i].energia<=0){
								pati[i].aika=0;
								break;
							}
						}

					}
				}
				if(player[j].alus.epatihajottaa &&
					 (j>=numero || !player[j].alus.epatiVoiHajota)){// || (player[j].alus.epatiVoiHajota && alus.patihajottaa)){
					for(k=0;k<player[j].alus.maxepadit;k++)// if(!(j==numero && k==i)) 
					if(player[j].epati[k].aika>0){
						double xpatiet=player[j].epati[k].x-pati[i].x;
						double ypatiet=player[j].epati[k].y-pati[i].y;
						if(xpatiet<player[j].alus.epatisade+alus.patisade
							&& ypatiet<player[j].alus.epatisade+alus.patisade)
						if(xpatiet*xpatiet+ypatiet*ypatiet<
						  pow((player[j].alus.epatisade+alus.patisade)*0.785,2)){
							if(player[j].alus.epatiVoiHajota){
								player[j].epati[k].energia-=alus.damage;
								if(player[j].alus.easelaji==4 || player[j].alus.easelaji==5){
									if(player[j].epati[k].energia>0)pati[i].aika=0;
									else pati[i].energia-=player[j].alus.epatienergia;
								}
								if(player[j].epati[k].energia<=0){ 
									player[j].epati[k].aika=0;
*/
/*									if(player[j].alus.easelaji==4){
										int scount=50;
										int j2=0;

										while(j2<sirpaleita && scount>0){
											if(sirpale[j2].aika<=0){
												scount--;
												sirpale[j2].x=player[j].epati[k].x;
												sirpale[j2].y=player[j].epati[k].y;
												sirpale[j2].xv=1;
												sirpale[j2].yv=1;
												while(sqrt(sirpale[j2].xv*sirpale[j2].xv+sirpale[j2].yv*sirpale[j2].yv)>0.3f){
													sirpale[j2].xv=random(25)/50.0f-0.25f;
													sirpale[j2].yv=random(25)/50.0f-0.25f;
												}
												sirpale[j2].maxaika=150;
												sirpale[j2].aika=random(100)+50;
												sirpale[j2].koko=random(20)/100.0f;
												sirpale[j2].red=random(100)/100.0f;
												sirpale[j2].green=random(100)/100.0f;
												sirpale[j2].blue=random(100)/100.0f;
											}
											j2++;
										}
									}*/
/*
								
								}
							}
							pati[i].energia-=player[j].alus.edamage;
							if(pati[i].energia<=0){
								pati[i].aika=0;
								//k=player[j].alus.maxepadit;
								//j=players;
								break;
							}
						}

					}
				}
			}
		}
	}


	if(alus.epatiVoiHajota) for(i=0;i<alus.maxepadit;i++){
		if(epati[i].aika>0){
			for(j=0;j<players;j++) if((!pistepeli) || player[j].pisteet>=0){// if(j!=numero){
				if(player[j].alus.patihajottaa &&
					(j>=numero || !player[j].alus.patiVoiHajota)){// || (player[j].alus.patiVoiHajota && alus.epatihajottaa)){
					for(k=0;k<player[j].alus.maxpadit;k++)//  if(!(j==numero && k==i)) 
					if(player[j].pati[k].aika>0){
						double xpatiet=player[j].pati[k].x-epati[i].x;
						double ypatiet=player[j].pati[k].y-epati[i].y;
//						if(sqrt(xpatiet*xpatiet+ypatiet*ypatiet)<
//						  (player[j].alus.patisade+alus.epatisade)/4.0f*PI){
						if(xpatiet<4
							&& ypatiet<4)
						if(xpatiet*xpatiet+ypatiet*ypatiet<
						  pow((player[j].alus.patisade+alus.epatisade)*0.785,2)){
							if(player[j].alus.patiVoiHajota){
								player[j].pati[k].energia-=alus.edamage;
								if(player[j].pati[k].energia<=0) player[j].pati[k].aika=0;
							}
							epati[i].energia-=player[j].alus.damage;
							if(alus.easelaji==4 || alus.easelaji==5){
								if(epati[i].energia>0)player[j].pati[k].aika=0;
								else player[j].pati[k].energia-=alus.epatienergia;
								if(player[j].pati[k].energia<=0) player[j].pati[k].aika=0;
							}
							if(epati[i].energia<=0){
								//k=player[j].alus.maxpadit;
								//j=players;
								epati[i].aika=0;
								break;
*/
								/*int scount=50;
								int j2=0;
								if(alus.easelaji==4){
									while(j2<sirpaleita && scount>0){
										if(sirpale[j2].aika<=0){
											scount--;
											sirpale[j2].x=epati[i].x;
											sirpale[j2].y=epati[i].y;
											sirpale[j2].xv=1;
											sirpale[j2].yv=1;
											while(sqrt(sirpale[j2].xv*sirpale[j2].xv+sirpale[j2].yv*sirpale[j2].yv)>0.3f){
												sirpale[j2].xv=random(25)/50.0f-0.25f;
												sirpale[j2].yv=random(25)/50.0f-0.25f;
											}
											sirpale[j2].maxaika=150;
											sirpale[j2].aika=random(100)+50;
											sirpale[j2].koko=random(20)/100.0f;
											sirpale[j2].red=random(100)/100.0f;
											sirpale[j2].green=random(100)/100.0f;
											sirpale[j2].blue=random(100)/100.0f;
										}
										j2++;
									}
								}*/
/*
							}
						}

					}
				}
				if(player[j].alus.epatihajottaa 
					&& (j>=numero || !player[j].alus.epatiVoiHajota)
					&& !(j==numero && alus.numero==17)
					&& !(player[j].alus.maxepadit>=180 && alus.maxepadit>=180)
					&& !(j==numero && alus.numero==22)){// || (player[j].alus.epatiVoiHajota && alus.epatihajottaa)){
					for(k=0;k<player[j].alus.maxepadit;k++)  if(!(j==numero && k==i)) if(player[j].epati[k].aika>0){
						double xpatiet=player[j].epati[k].x-epati[i].x;
						double ypatiet=player[j].epati[k].y-epati[i].y;
						if(xpatiet<4
							&& ypatiet<4)
						if(xpatiet*xpatiet+ypatiet*ypatiet<
						  pow((player[j].alus.epatisade+alus.epatisade)*0.785,2)){
							if(player[j].alus.epatiVoiHajota){
								player[j].epati[k].energia-=alus.edamage;
								if(player[j].epati[k].energia<=0) player[j].epati[k].aika=0;
							}
							epati[i].energia-=player[j].alus.edamage;
							if(alus.easelaji==4 || alus.easelaji==5){
								epati[i].energia-=player[j].alus.epatienergia;
							}
							if(alus.easelaji==4 || alus.easelaji==5){
								if(epati[i].energia>0)player[j].epati[k].aika=0;
								else player[j].epati[k].energia-=alus.epatienergia;
								if(player[j].epati[k].energia<=0) player[j].epati[k].aika=0;
							}
							if(epati[i].energia<0) epati[i].energia=0;
							if(player[j].epati[k].energia<0) player[j].epati[k].energia=0;
							if(epati[i].energia<=0){
								epati[i].aika=0;
								break;
							}

						}

					}
				}
			}
		}
	}
}

*/
void Player::liikutaPadit(){
	int i,j,k;
	if(alus.numero==23){
		for (i=0;i<alus.maxpadit;i++){
			if(pati[i].aika>0){ 
				if(!joinpeli)pati[i].aika--;
				if(pati[i].x>maxx) pati[i].xv-=0.02;
				if(pati[i].x<minx) pati[i].xv+=0.02;
				if(pati[i].y>maxy) pati[i].yv-=0.02;
				if(pati[i].y<miny) pati[i].yv+=0.02;
				pati[i].x+=pati[i].xv;
				pati[i].y+=pati[i].yv;
				double tempetaisyys=pati[i].x*pati[i].x+pati[i].y*pati[i].y;
				if(!joinpeli) if(tempetaisyys<planeettasadetoiseen) pati[i].aika=0;
				else if(tempetaisyys<gravitysadetoiseen){
					pati[i].xv-=pati[i].x/(tempetaisyys)*patigravity;
					pati[i].yv-=pati[i].y/(tempetaisyys)*patigravity;
				}
				for(j=0;j<players;j++) if((!pistepeli) || player[j].pisteet>=0){
//					if(j!=numero)
					if(player[j].tormaa(pati[i], alus)){
						if(osumisaani!=NULL){
							if(efektitpaalla) osumisaani->play();
						}
						player[j].energia-=alus.damage;
						double xtemp=player[j].xvauhti;
						double ytemp=player[j].yvauhti;
						player[j].xvauhti+=pati[i].xv*alus.patipotkasu/player[j].alus.paino*alus.paino;
						player[j].yvauhti+=pati[i].yv*alus.patipotkasu/player[j].alus.paino*alus.paino;
						pati[i].xv-=xtemp*player[j].alus.paino/200.0f;
						pati[i].yv-=ytemp*player[j].alus.paino/200.0f;


					}
				}
			}
		}
	}
	else if(alus.numero==20){
		for (i=0;i<alus.maxpadit;i++){
			if(pati[i].aika>0){ 
				pati[i].aika--;
				pati[i].x+=pati[i].xv;
				pati[i].y+=pati[i].yv;
				double tempetaisyys=pati[i].x*pati[i].x+pati[i].y*pati[i].y;
				if(!joinpeli)if(tempetaisyys<planeettasadetoiseen) pati[i].aika=0;
				if(tempetaisyys<gravitysadetoiseen){
					pati[i].xv-=pati[i].x/(tempetaisyys)*patigravity;
					pati[i].yv-=pati[i].y/(tempetaisyys)*patigravity;
				}
				for(j=0;j<players;j++) if(j!=numero) if((!pistepeli) || player[j].pisteet>=0){
					if(player[j].tormaa(pati[i], alus)){
						if(osumisaani!=NULL){
							if(efektitpaalla) osumisaani->play();
						}
						if(!joinpeli){
							player[j].energia-=alus.damage;
							pati[i].aika=0;
							player[j].energia-=alus.damage;
							player[j].xvauhti+=pati[i].xv*alus.patipotkasu;
							player[j].yvauhti+=pati[i].yv*alus.patipotkasu;
						}
					}
				}
				pati[i].kulma+=cos((alus.patiaika-pati[i].aika)/10.0f);
				pati[i].xv+=cos(pati[i].kulma+PI/2)*cos((alus.patiaika-pati[i].aika/10.0f))*0.03;
				pati[i].yv+=sin(pati[i].kulma+PI/2)*cos((alus.patiaika-pati[i].aika/10.0f))*0.03;
			}
		}
	}else if(alus.numero==16){
		for (i=0;i<alus.maxpadit;i++){
			if(pati[i].aika>0){ 
				if(!joinpeli)pati[i].aika--;
				pati[i].x+=pati[i].xv;
				pati[i].y+=pati[i].yv;
				double tempetaisyys=pati[i].x*pati[i].x+pati[i].y*pati[i].y;
				if(!joinpeli) if(tempetaisyys<planeettasadetoiseen) pati[i].aika=0;
				if(tempetaisyys<gravitysadetoiseen){
					pati[i].xv-=pati[i].x/(tempetaisyys)*patigravity;
					pati[i].yv-=pati[i].y/(tempetaisyys)*patigravity;
				}
				for(j=0;j<players;j++) if(j!=numero) if((!pistepeli) || player[j].pisteet>=0){
					if(player[j].tormaa(pati[i], alus)){
						if(osumisaani!=NULL){
							if(efektitpaalla) osumisaani->play();
						}
						if(!joinpeli){
							player[j].energia-=alus.damage;
							pati[i].aika=0;
							player[j].energia-=alus.damage;
							player[j].xvauhti+=pati[i].xv*alus.patipotkasu;
							player[j].yvauhti+=pati[i].yv*alus.patipotkasu;
						}
					}
				}
				double vauhti=sqrt(pati[i].xv*pati[i].xv+pati[i].yv*pati[i].yv);
				pati[i].kulma=atan2(pati[i].yv,pati[i].xv);
				int gahkulma=random(2);
				if(gahkulma==1) pati[i].kulma+=PI/180.0f*2.0f;
				else pati[i].kulma-=PI/180.0f*2.0f;
				pati[i].xv=cos(pati[i].kulma)*vauhti;
				pati[i].yv=sin(pati[i].kulma)*vauhti;
				pati[i].kulma=atan2(pati[i].yv,pati[i].xv)*180.0f/PI;

			}
		}
	}else if(alus.aselaji==15){
		for (i=0;i<alus.maxpadit;i++){
			if(pati[i].aika>0){ 
				if(!joinpeli)pati[i].aika--;
				pati[i].x+=random(100)/100.0f*alus.patinopeus-alus.patinopeus/2;
				pati[i].y+=random(100)/100.0f*alus.patinopeus-alus.patinopeus/2;
				double tempetaisyys=pati[i].x*pati[i].x+pati[i].y*pati[i].y;
				if(!joinpeli) if(tempetaisyys<planeettasadetoiseen) pati[i].aika=0;
				if(tempetaisyys<gravitysadetoiseen){
					pati[i].xv-=pati[i].x/(tempetaisyys)*patigravity;
					pati[i].yv-=pati[i].y/(tempetaisyys)*patigravity;
				}
//				double etaisyys=maxx*4;
				for(j=0;j<players;j++) if(j!=numero)if((!pistepeli) || player[j].pisteet>=0){
					if(player[j].tormaa(pati[i], alus)){
						if(osumisaani!=NULL){
							if(efektitpaalla) osumisaani->play();
						}
						if(!joinpeli){
							player[j].energia-=alus.damage;
							pati[i].aika=0;
							player[j].energia-=alus.damage;
							player[j].xvauhti+=pati[i].xv*alus.patipotkasu;
							player[j].yvauhti+=pati[i].yv*alus.patipotkasu;
						}
					}
				}
			}
		}
	}
	else if(alus.aselaji==14){
		for (i=0;i<alus.maxpadit;i++){
			if(pati[i].aika>0){ 
				pati[i].aika--;
				pati[i].x+=pati[i].xv;
				pati[i].y+=pati[i].yv;
				double tempetaisyys=pati[i].x*pati[i].x+pati[i].y*pati[i].y;
				if(!joinpeli) if(tempetaisyys<planeettasadetoiseen) pati[i].aika=0;
				if(tempetaisyys<gravitysadetoiseen){
					pati[i].xv-=pati[i].x/(tempetaisyys)*patigravity;
					pati[i].yv-=pati[i].y/(tempetaisyys)*patigravity;
				}
//				double etaisyys=maxx*4;
				for(j=0;j<players;j++)if((!pistepeli) || player[j].pisteet>=0){
					if(player[j].tormaa(pati[i], alus)){
						if(osumisaani!=NULL){
							player[j].energia-=alus.damage;
							if(efektitpaalla) osumisaani->play();
						}
						if(!joinpeli){
							pati[i].aika=0;
							player[j].energia-=alus.damage;
							player[j].xvauhti+=pati[i].xv*alus.patipotkasu;
							player[j].yvauhti+=pati[i].yv*alus.patipotkasu;
						}
					}
				}
//				double vauhti=sqrt(pati[i].xv*pati[i].xv+pati[i].yv*pati[i].yv);
				pati[i].xv+=alus.patinopeus*cos(pati[i].kulma/180*PI);//*vauhti;
				pati[i].yv+=alus.patinopeus*sin(pati[i].kulma/180*PI);//*vauhti;
				pati[i].xv/=1.0f+kitka*1*(fabs(pati[i].xv)+1.0f);
				pati[i].yv/=1.0f+kitka*1*(fabs(pati[i].yv)+1.0f);
//				pati[i].kulma=atan2(pati[i].yv,pati[i].xv)*180.0f/PI;
			}
		}
	}
	else if(alus.aselaji==7){
		for (i=0;i<alus.maxpadit;i++){
			if(pati[i].aika>0){ 
				if(!joinpeli) pati[i].aika--;
				pati[i].x+=pati[i].xv;
				pati[i].y+=pati[i].yv;
				double tempetaisyys=pati[i].x*pati[i].x+pati[i].y*pati[i].y;
				if(!joinpeli) if(tempetaisyys<planeettasadetoiseen) pati[i].aika=0;
				if(tempetaisyys<gravitysadetoiseen){
					pati[i].xv-=pati[i].x/(tempetaisyys)*patigravity;
					pati[i].yv-=pati[i].y/(tempetaisyys)*patigravity;
				}
				double etaisyys=maxx*4;
				k=0;
				for(j=0;j<players;j++)if((!pistepeli) || player[j].pisteet>=0){
					if(player[j].tormaa(pati[i], alus)){
						if(osumisaani!=NULL){
							player[j].energia-=alus.damage;
//							if(player[j].energia<0)player[j].energia=0;
							if(efektitpaalla) osumisaani->play();
						}
						if(!joinpeli){
							pati[i].aika=0;
							player[j].energia-=alus.damage;
							player[j].xvauhti+=pati[i].xv*alus.patipotkasu;
							player[j].yvauhti+=pati[i].yv*alus.patipotkasu;
						}
					}
					if(j!=numero && player[j].tiimi!=tiimi){
						double xet=pati[i].x-player[j].x;
						double yet=pati[i].y-player[j].y;
						double etaisyys2=sqrt(xet*xet+yet*yet);
						if(etaisyys2<etaisyys){
							k=j;
							etaisyys=etaisyys2;
						}
					}
				}
				pati[i].xv+=(player[k].x-pati[i].x)/etaisyys*alus.patinopeus;
				pati[i].yv+=(player[k].y-pati[i].y)/etaisyys*alus.patinopeus;
				pati[i].xv/=1.0f+kitka*1*(fabs(pati[i].xv)+1.0f);
				pati[i].yv/=1.0f+kitka*1*(fabs(pati[i].yv)+1.0f);
				pati[i].kulma=atan2(player[k].y-pati[i].y,player[k].x-pati[i].x)*180.0f/PI;
			}
		}
	}else if(alus.aselaji==4){
		for (i=0;i<alus.maxpadit;i++){
			if(pati[i].aika>0){ 
				if(!joinpeli) pati[i].aika--;
				pati[i].x+=pati[i].xv;
				pati[i].y+=pati[i].yv;
				double tempetaisyys=pati[i].x*pati[i].x+pati[i].y*pati[i].y;
				if(!joinpeli) if(tempetaisyys<planeettasadetoiseen) pati[i].aika=0;
				if(tempetaisyys<gravitysadetoiseen){
					pati[i].xv-=pati[i].x/(tempetaisyys)*patigravity;
					pati[i].yv-=pati[i].y/(tempetaisyys)*patigravity;
				}

				if(i==0){
					if(pati[i].x+pati[i].xv>maxx) pati[i].xv-=0.03;
					if(pati[i].y+pati[i].yv>maxy) pati[i].yv-=0.03;
					if(pati[i].x+pati[i].xv<minx) pati[i].xv+=0.03;
					if(pati[i].y+pati[i].yv<miny) pati[i].yv+=0.03;
				}
				if(i==0 && (pati[i].aika<=0 || (!joinpeli && pfire==false) || (joinpeli && joinase==false))){
					if(efektitpaalla) eampumisaani->play();
					if(!joinpeli) pati[i].aika=0;
					int pcount=30;
					if(!joinpeli) for(k=1;k<alus.maxpadit;k++){
						if(pati[k].aika==0){
							pcount--;
							pati[k].x=pati[0].x+cos((double)((double)pcount/(double)30.0f*2.0f*PI))*4.0f;
							pati[k].y=pati[0].y+sin((double)((double)pcount/(double)30.0f*2.0f*PI))*4.0f;
							pati[k].xv=cos((double)((double)pcount/(double)30.0f*2.0f*PI))*0.15f;
							pati[k].yv=sin((double)((double)pcount/(double)30.0f*2.0f*PI))*0.15f;
							pati[k].aika=40;
							pati[k].energia=alus.patienergia/8.0f;
						}
						if(pcount<=0)k=alus.maxpadit;
					}
				}

				for(j=0;j<players;j++)if((!pistepeli) || player[j].pisteet>=0){
					if(j!=numero)
					if(player[j].tormaa(pati[i], alus)){
						if(!joinpeli) pati[i].aika=0;
						if(i==0){
							if(efektitpaalla) eosumisaani->play();
							player[j].energia-=alus.damage;
						} else{
						if(efektitpaalla) osumisaani->play();
							player[j].energia-=alus.damage/8.0f;
						}
						player[j].xvauhti+=pati[i].xv*alus.patipotkasu;
						player[j].yvauhti+=pati[i].yv*alus.patipotkasu;
					}
				}

			}
		}
	}else{
		for (i=0;i<alus.maxpadit;i++){
			if(pati[i].aika>0){ 
				pati[i].aika--;
				pati[i].x+=pati[i].xv;
				pati[i].y+=pati[i].yv;
				if(alus.numero==22){
					pati[i].kulma+=PI/45.0f;
				}
				double tempetaisyys=pati[i].x*pati[i].x+pati[i].y*pati[i].y;
				if(tempetaisyys<planeettasadetoiseen && !joinpeli) pati[i].aika=0;
				if(tempetaisyys<gravitysadetoiseen){
					pati[i].xv-=pati[i].x/(tempetaisyys)*patigravity;
					pati[i].yv-=pati[i].y/(tempetaisyys)*patigravity;
				}
				for(j=0;j<players;j++)if((!pistepeli) || player[j].pisteet>=0){
					if(j!=numero)
					if(player[j].tormaa(pati[i], alus)){
						if(osumisaani!=NULL){
							if(efektitpaalla) osumisaani->play();
						}
						if(!joinpeli) pati[i].aika=0;
						if(i==0 && alus.numero==13){
							ampumisviive=alus.ampumisnopeus;
						}
						if(alus.aselaji==5 || alus.aselaji==1){
							int scount=150+sets->detail*50;
							int j2=0;
							while(j2<sirpaleita && scount>0){
								if(sirpale[j2].aika<=0){
									scount--;
									sirpale[j2].x=player[j].x;
									sirpale[j2].y=player[j].y;
									sirpale[j2].xv=1;
									sirpale[j2].yv=1;
									sirpale[j2].z=0;
									sirpale[j2].zv=0;
									while(sirpale[j2].xv*sirpale[j2].xv+sirpale[j2].yv*sirpale[j2].yv>0.04f){
										sirpale[j2].xv=random(400)/1000.0f-0.2f;
										sirpale[j2].yv=random(400)/1000.0f-0.2f;
									}
									sirpale[j2].xv+=pati[i].xv;
									sirpale[j2].yv+=pati[i].yv;
									sirpale[j2].maxaika=75;
									sirpale[j2].aika=random(50)+25;
									sirpale[j2].koko=random(30)/100.0f*2;
									sirpale[j2].muoto=0;
									sirpale[j2].red=random(100)/50.0f+0.5f;
									sirpale[j2].green=random(100)/100.0f;
									sirpale[j2].blue=random(100)/100.0f;
								}
								j2++;
							}
						}
						if(alus.aselaji==13){
							player[j].energia-=alus.damage/2000.0f*pati[i].energia;
						}else player[j].energia-=alus.damage;
						player[j].xvauhti+=pati[i].xv*alus.patipotkasu;
						player[j].yvauhti+=pati[i].yv*alus.patipotkasu;
					}
				}
			}
		}
	}
}


void Player::liikutaePadit(){
	int i,j,k,patij;
	double tempetaisyys;
	double count, maxcount;
	double alus23ase=25;
	switch(alus.numero){
	case 23:
		for(i=0;i<alus.maxepadit;i++) if(epati[i].aika>0){
			epati[i].aika--;
			if(epati[i].aika==1){
				epati[i].aika=0;
				if(efektitpaalla) eosumisaani->play();
				for(k=0;k<players;k++){
					double xet=player[k].x-epati[i].x;
					double yet=player[k].y-epati[i].y;
					if(fabs(xet)<alus23ase && fabs(yet)<alus23ase){
						double etaisyys=sqrt(xet*xet+yet*yet);
						if(etaisyys<alus23ase && !(player[k].alus.easelaji==0 && player[k].suojapaalla)) if((!pistepeli) || player[k].pisteet>=0)player[k].energia-=alus.edamage-etaisyys*3.2f;
					}
					if(player[k].alus.patiVoiHajota)if((!pistepeli) || player[k].pisteet>=0){
						for(j=0;j<player[k].alus.maxpadit;j++){
							if(player[k].pati[j].aika>0){
								double xet=player[k].pati[j].x-epati[i].x;
								double yet=player[k].pati[j].y-epati[i].y;
								if(fabs(xet)<alus23ase && fabs(yet)<alus23ase){
									double etaisyys=sqrt(xet*xet+yet*yet);
									if(!joinpeli) if(etaisyys<alus23ase){
										player[k].pati[j].energia-=alus.edamage-etaisyys*3.2f;
										if(player[k].pati[j].energia<=0) player[k].pati[j].aika=0;
									}
								}
							}
						}
					}
					if(player[k].alus.epatiVoiHajota)if((!pistepeli) || player[k].pisteet>=0){
						for(j=0;j<player[k].alus.maxepadit;j++){
							if(player[k].epati[j].aika>0){
								double xet=player[k].epati[j].x-epati[i].x;
								double yet=player[k].epati[j].y-epati[i].y;
								if(fabs(xet)<alus23ase && fabs(yet)<alus23ase){
									double etaisyys=sqrt(xet*xet+yet*yet);
									if(!joinpeli) if(etaisyys<alus23ase){
										player[k].epati[j].energia-=alus.edamage-etaisyys*4.0f;
										if(player[k].epati[j].energia<=0) player[k].epati[j].aika=0;
									}
								}
							}
						}
					}
				}
				int scount=200+sets->detail*50;
				int j2=0;
				while(j2<sirpaleita && scount>0){
					if(sirpale[j2].aika<=0){
						scount--;
						sirpale[j2].x=epati[i].x;
						sirpale[j2].y=epati[i].y;
						sirpale[j2].xv=1;
						sirpale[j2].yv=1;
						sirpale[j2].z=0;
						sirpale[j2].zv=0;
						while(sirpale[j2].xv*sirpale[j2].xv+sirpale[j2].yv*sirpale[j2].yv>0.0625f){
							sirpale[j2].xv=random(100)/200.0f-0.25f;
							sirpale[j2].yv=random(100)/200.0f-0.25f;
						}
						sirpale[j2].maxaika=400;
						sirpale[j2].aika=random(300)+100;
						sirpale[j2].koko=random(70)/100.0f;
						sirpale[j2].muoto=0;
						sirpale[j2].red=random(50)/100.0f+0.5f;
						sirpale[j2].green=random(30)/100.0f;
						sirpale[j2].blue=random(30)/100.0f;
					}
					j2++;
				}		
			}
		}
		break;
	case 22:
		for (i=0;i<alus.maxepadit;i++){
			if(epati[i].aika>0){ 
				epati[i].aika--;
				epati[i].xv+=random(100)/1000.0f-0.05f;
				epati[i].yv+=random(100)/1000.0f-0.05f;
				epati[i].x+=epati[i].xv;
				epati[i].y+=epati[i].yv;
				tempetaisyys=epati[i].x*epati[i].x+epati[i].y*epati[i].y;
				if(!joinpeli) if(tempetaisyys<planeettasadetoiseen) epati[i].aika=0;
				else if(tempetaisyys<gravitysadetoiseen){
					epati[i].xv-=epati[i].x/(tempetaisyys)*patigravity;
					epati[i].yv-=epati[i].y/(tempetaisyys)*patigravity;
				}
				for(j=0;j<players;j++)if((!pistepeli) || player[j].pisteet>=0){
					if(!joinpeli) if(player[j].tormaa(epati[i], alus)){
						epati[i].aika=0;
						if(!(player[j].alus.easelaji==0 && player[j].suojapaalla))player[j].energia-=alus.edamage;
						player[j].xvauhti+=epati[i].xv*alus.epatipotkasu;
						player[j].yvauhti+=epati[i].yv*alus.epatipotkasu;
					}
				}
			}
		}
		break;
	case 21:
		for (i=0;i<alus.maxepadit;i++){
			if(epati[i].aika==1){ 
				if(!joinpeli){
					epati[i].aika=0;
					if(epati[i].r==1){
						count=30;
						maxcount=30;
						for(j=0;j<alus.maxepadit;j++){
							if(epati[j].aika<=0){
								epati[j].x=epati[i].x+cos((kulma+count*360.0/maxcount)/180*PI)*alus.sade*1.3f;
								epati[j].y=epati[i].y+sin((kulma+count*360.0/maxcount)/180*PI)*alus.sade*1.3f;
								epati[j].xv=cos((kulma+count*360.0/maxcount)/180*PI)*alus.easevauhti*(random(35)/100.0f+0.15)+epati[i].xv;
								epati[j].yv=sin((kulma+count*360.0/maxcount)/180*PI)*alus.easevauhti*(random(35)/100.0f+0.15)+epati[i].yv;
								epati[j].aika=alus.easekesto/2;
								epati[j].kulma=0;
								epati[j].energia=alus.epatienergia;
								epati[j].r=0;
								epati[j].g=1;
								count--;
								if(count<=0) j=alus.maxepadit;
							}
						}
					}else epati[i].aika=0;
				}
			}else{
				if(epati[i].aika>0){ 
					epati[i].aika--;
					if(epati[i].g==1){
						epati[i].xv+=random(100)/1000.0f-0.05f;
						epati[i].yv+=random(100)/1000.0f-0.05f;
					}
					epati[i].x+=epati[i].xv;
					epati[i].y+=epati[i].yv;
					tempetaisyys=epati[i].x*epati[i].x+epati[i].y*epati[i].y;
					if(!joinpeli) if(tempetaisyys<planeettasadetoiseen) epati[i].aika=0;
					else if(tempetaisyys<gravitysadetoiseen){
						epati[i].xv-=epati[i].x/(tempetaisyys)*patigravity;
						epati[i].yv-=epati[i].y/(tempetaisyys)*patigravity;
					}
					if(!joinpeli) for(j=0;j<players;j++)if((!pistepeli) || player[j].pisteet>=0){
						if(player[j].tormaa(epati[i], alus)){
							epati[i].aika=0;
							if(!(player[j].alus.easelaji==0 && player[j].suojapaalla))player[j].energia-=alus.edamage;
							player[j].xvauhti+=epati[i].xv*alus.epatipotkasu;
							player[j].yvauhti+=epati[i].yv*alus.epatipotkasu;
						}
					}
				}
			}
		}
		break;
	case 20:
		for (i=0;i<alus.maxepadit;i++){
			if(epati[i].aika>0){ 
				epati[i].aika--;
				epati[i].x+=epati[i].xv;
				epati[i].y+=epati[i].yv;
				tempetaisyys=epati[i].x*epati[i].x+epati[i].y*epati[i].y;
				if(!joinpeli) if(tempetaisyys<planeettasadetoiseen) epati[i].aika=0;
				else if(tempetaisyys<gravitysadetoiseen){
					epati[i].xv-=epati[i].x/(tempetaisyys)*patigravity;
					epati[i].yv-=epati[i].y/(tempetaisyys)*patigravity;
				}
				if(!joinpeli) for(j=0;j<players;j++)if((!pistepeli) || player[j].pisteet>=0){
					if(player[j].tormaa(epati[i], alus)){
						epati[i].aika=0;
						if(!(player[j].alus.easelaji==0 && player[j].suojapaalla))player[j].energia-=alus.edamage;
						player[j].xvauhti+=epati[i].xv*alus.epatipotkasu;
						player[j].yvauhti+=epati[i].yv*alus.epatipotkasu;
					}
				}
			}
		}
		break;
	case 17:
		for(i=0;i<alus.maxepadit;i++){
			if(epati[i].aika>0){ 
				epati[i].aika--;
				double etaisyys=maxx*2+maxy*2;
//				double xetaisyys;
//				double yetaisyys;
				for(k=0;k<players;k++) if(k!=numero)if((!pistepeli) || player[k].pisteet>=0){
					double xet=epati[i].x-player[k].x;
					double yet=epati[i].y-player[k].y;
					double etaisyys2=xet*xet+yet*yet;
					if(!joinpeli) if(etaisyys2<pow((alus.epatisade+player[k].alus.sade)*0.785,2)){
						if(!(player[k].alus.easelaji==0 && player[k].suojapaalla)){
							player[k].energia-=alus.edamage;
							epati[i].aika=0;
							if(efektitpaalla) eosumisaani->play();
						}
					}
				}
			}
		}
		break;
	case 16:
		patij=0;
		for(i=0;i<alus.maxepadit;i++){
			if(epati[i].aika==1){
				epati[i].aika=0;
				if(epati[i].r==1){
					double count=20;
					if(!joinpeli) for(;patij<alus.maxepadit,count>0;patij++){
						if(epati[patij].aika<=0){
							count--;
							epati[patij].x=epati[i].x+cos(count/20.0f*2.0f*PI)*3.5f;
							epati[patij].y=epati[i].y+sin(count/20.0f*2.0f*PI)*3.5f;
							epati[patij].xv=cos(count/20.0f*2.0f*PI)*0.4;
							epati[patij].yv=sin(count/20.0f*2.0f*PI)*0.4;
							epati[patij].aika=alus.easekesto/2;
							epati[patij].r=0;
							epati[patij].g=1;
							epati[patij].b=0;
							//if(count<=0) j=alus.maxepadit;
						}
					}
				}else if(epati[i].g==1){
					double count=15;
					if(!joinpeli) for(;patij<alus.maxepadit,count>0;patij++){
						if(epati[patij].aika<=0){
							count--;
							epati[patij].x=epati[i].x+cos(count/15.0f*2.0f*PI)*3.5f;
							epati[patij].y=epati[i].y+sin(count/15.0f*2.0f*PI)*3.5f;
							epati[patij].xv=cos(count/15.0f*2.0f*PI)*0.3;
							epati[patij].yv=sin(count/15.0f*2.0f*PI)*0.3;
							epati[patij].aika=alus.easekesto/2;
							epati[patij].r=0;
							epati[patij].g=0;
							epati[patij].b=1;
							//if(count<=0) j=alus.maxepadit;
						}
					}
				}
			}else if(epati[i].aika>0){
				epati[i].aika--;
				epati[i].x+=epati[i].xv;
				epati[i].y+=epati[i].yv;
/*				double tempetaisyys=sqrt(epati[i].x*epati[i].x+epati[i].y*epati[i].y);
				if(tempetaisyys<planeettasade) epati[i].aika=0;
				if(tempetaisyys<gravitysade){
					epati[i].xv-=epati[i].x/(tempetaisyys*tempetaisyys)*patigravity;
					epati[i].yv-=epati[i].y/(tempetaisyys*tempetaisyys)*patigravity;
				}*/
				for(j=0;j<players;j++)if((!pistepeli) || player[j].pisteet>=0){
					if(player[j].x-epati[i].x<3
						&& player[j].y-epati[i].y<3)
					if(player[j].tormaa(epati[i], alus)){
						if(efektitpaalla && eosumisaani!=NULL) eosumisaani->play();
						if(!joinpeli){
							epati[i].aika=0;
							player[j].energia-=alus.edamage;
						}
					}
				}
			}
		}
		break;
	case 4:
		for(i=0;i<alus.maxepadit;i++){
			if(epati[i].aika>0){ 
				epati[i].aika--;
				double etaisyys=maxx*2+maxy*2;
				double xetaisyys;
				double yetaisyys;
				j=0;
				for(k=0;k<players;k++)if((!pistepeli) || player[k].pisteet>=0) if(player[k].tiimi!=tiimi){
					double xet=epati[i].x-player[k].x;
					double yet=epati[i].y-player[k].y;
					double etaisyys2=sqrt(xet*xet+yet*yet);
					if(k!=numero){
						if(etaisyys2<etaisyys){
							j=k;
							etaisyys=etaisyys2;
							xetaisyys=xet;
							yetaisyys=yet;
						}
					}
				}
				if(etaisyys<(alus.epatisade+player[j].alus.sade)*0.785){
					if(efektitpaalla) eosumisaani->play();
					player[j].fuel-=alus.edamage;
					if(player[j].fuel<0) player[j].fuel=0;
					if(xetaisyys==0) xetaisyys=0.01;
					if(yetaisyys==0) yetaisyys=0.01;
					epati[i].xv=player[j].xvauhti+(xetaisyys/fabs(xetaisyys))*0.05f;
					epati[i].yv=player[j].yvauhti+(yetaisyys/fabs(yetaisyys))*0.05f;
				}
				epati[i].xv/=1.0f+kitka*1*(fabs(xvauhti)+1.0f);
				epati[i].yv/=1.0f+kitka*1*(fabs(yvauhti)+1.0f);

				epati[i].xv-=xetaisyys/etaisyys*alus.easevauhti;
				epati[i].yv-=yetaisyys/etaisyys*alus.easevauhti;
				epati[i].x+=epati[i].xv;
				epati[i].y+=epati[i].yv;
				epati[i].kulma=atan2(yetaisyys,xetaisyys)*180.0f/PI-180.0f;
				etaisyys=epati[i].x*epati[i].x+epati[i].y*epati[i].y;
				if(!joinpeli) if(etaisyys<planeettasadetoiseen){
					epati[i].aika=0;

					int scount=50;
					int j2=0;

					while(j2<sirpaleita && scount>0){
						if(sirpale[j2].aika<=0){
							scount--;
							sirpale[j2].x=epati[i].x;
							sirpale[j2].y=epati[i].y;
							sirpale[j2].xv=1;
							sirpale[j2].yv=1;
							sirpale[j2].zv=0;
							sirpale[j2].z=0;
							while(sirpale[j2].xv*sirpale[j2].xv+sirpale[j2].yv*sirpale[j2].yv>0.09f){
								sirpale[j2].xv=random(25)/50.0f-0.25f;
								sirpale[j2].yv=random(25)/50.0f-0.25f;
							}
							sirpale[j2].maxaika=150;
							sirpale[j2].aika=random(100)+50;
							sirpale[j2].muoto=0;
							sirpale[j2].red=random(100)/100.0f;
							sirpale[j2].green=random(100)/100.0f;
							sirpale[j2].blue=random(100)/100.0f;
						}
						j2++;
					}

				}
				if(etaisyys<gravitysadetoiseen){
					epati[i].xv-=epati[i].x/(etaisyys)*patigravity;
					epati[i].yv-=epati[i].y/(etaisyys)*patigravity;
				}

			}
		}
		break;
	case 3:
		for (i=0;i<alus.maxepadit;i++){
			if(epati[i].aika>0){ 
				epati[i].aika--;
				double tempetaisyys=epati[i].x*epati[i].x+epati[i].y*epati[i].y;
				if(!joinpeli) if(tempetaisyys<planeettasadetoiseen) epati[i].aika=0;
				double etaisyys=maxx*2+maxy*2;
				double xetaisyys;
				double yetaisyys;
				j=0;
				for(k=0;k<players;k++)if((!pistepeli) || player[k].pisteet>=0){
					double xet=epati[i].x-player[k].x;
					double yet=epati[i].y-player[k].y;
					double etaisyys2=sqrt(xet*xet+yet*yet);
					if(k!=numero && player[k].tiimi!=tiimi){
						if(etaisyys2<etaisyys){
							j=k;
							etaisyys=etaisyys2;
							xetaisyys=xet;
							yetaisyys=yet;
						}

					}
					if(etaisyys2<(alus.epatisade+player[k].alus.sade)*0.6f){
						if(!joinpeli){
							epati[i].aika=0;
							if(!(player[k].alus.easelaji==0 && player[k].suojapaalla))player[k].energia-=alus.edamage;
						}
						if(efektitpaalla) eosumisaani->play();
					}
				}
				epati[i].x-=xetaisyys/etaisyys*alus.easevauhti;
				epati[i].y-=yetaisyys/etaisyys*alus.easevauhti;
			}
		}
		break;
	case 1:
		for (i=0;i<alus.maxepadit;i++){
			if(epati[i].aika>0){ 
				epati[i].aika--;
				epati[i].x+=epati[i].xv;
				epati[i].y+=epati[i].yv;
				double tempetaisyys=epati[i].x*epati[i].x+epati[i].y*epati[i].y;
				if(!joinpeli) if(tempetaisyys<planeettasadetoiseen) epati[i].aika=0;
				else if(tempetaisyys<gravitysadetoiseen){
					epati[i].xv-=epati[i].x/(tempetaisyys)*patigravity;
					epati[i].yv-=epati[i].y/(tempetaisyys)*patigravity;
				}
				
				if(!joinpeli) for(j=0;j<players;j++)if((!pistepeli) || player[j].pisteet>=0){
					if(j!=numero)
					if(player[j].tormaa(epati[i], alus)){
						epati[i].aika=0;
						if(!(player[j].alus.easelaji==0 && player[j].suojapaalla))player[j].energia-=alus.edamage;
						player[j].xvauhti+=epati[i].xv*alus.epatipotkasu;
						player[j].yvauhti+=epati[i].yv*alus.epatipotkasu;
					}
				}
			}
		}
		break;
	case 2:
		for (i=0;i<alus.maxepadit;i++){
			if(epati[i].aika>0){ 
				epati[i].aika--;
				epati[i].x+=epati[i].xv;
				epati[i].y+=epati[i].yv;
				double tempetaisyys=epati[i].x*epati[i].x+epati[i].y*epati[i].y;
				if(!joinpeli) if(tempetaisyys<planeettasadetoiseen) epati[i].aika=0;
				else if(tempetaisyys<gravitysadetoiseen){
					epati[i].xv-=epati[i].x/(tempetaisyys)*patigravity;
					epati[i].yv-=epati[i].y/(tempetaisyys)*patigravity;
				}
				if(!joinpeli) for(j=0;j<players;j++)if((!pistepeli) || player[j].pisteet>=0){
					if(j!=numero)
					if(player[j].tormaa(epati[i], alus)){
						epati[i].aika=0;
						if(!(player[j].alus.easelaji==0 && player[j].suojapaalla))player[j].energia-=alus.edamage;
						player[j].xvauhti+=epati[i].xv*alus.epatipotkasu;
						player[j].yvauhti+=epati[i].yv*alus.epatipotkasu;
					}
				}
			}
		}
		break;
	case 5:
		for (i=0;i<alus.maxepadit;i++){
			if(epati[i].aika>0){ 
				epati[i].aika--;
				double tempetaisyys=epati[i].x*epati[i].x+epati[i].y*epati[i].y;
				if(!joinpeli) if(tempetaisyys<planeettasadetoiseen) epati[i].aika=0;
				if(epati[i].ampumisviive>0)epati[i].ampumisviive--;
				if(epati[i].aika<alus.easekesto/3.0f){
					epati[i].x+=(x-epati[i].x)/sqrt((x-epati[i].x)*(x-epati[i].x)+(y-epati[i].y)*(y-epati[i].y))*alus.easevauhti;
					epati[i].y+=(y-epati[i].y)/sqrt((x-epati[i].x)*(x-epati[i].x)+(y-epati[i].y)*(y-epati[i].y))*alus.easevauhti;
					if(!joinpeli) if((x-epati[i].x)*(x-epati[i].x)+(y-epati[i].y)*(y-epati[i].y)<pow(alus.sade,2)){
						epati[i].aika=0;
						energia++;
					}
				}else{
					j=0;
					double etaisyys=maxx*2+maxy*2;
					double xetaisyys;
					double yetaisyys;
					for(k=0;k<players;k++)if((!pistepeli) || player[k].pisteet>=0) if(player[k].tiimi!=tiimi){
						if(k!=numero){
							double xet=epati[i].x-player[k].x;
							double yet=epati[i].y-player[k].y;
							double etaisyys2=sqrt(xet*xet+yet*yet);
							if(etaisyys2<etaisyys){
								etaisyys=etaisyys2;
								j=k;
								xetaisyys=xet;
								yetaisyys=yet;
							}
						}
					}
					if(numero==0 && j==0) j=1;
					if(numero==1 && j==1) j=0;
//					epati[i].kulma=atan2(yetaisyys, xetaisyys)*180/PI;
					epati[i].kulma=90;
					if(etaisyys<0.8f){ if(!joinpeli) epati[i].aika=0;
					}else if(etaisyys<player[j].alus.sade+1.2f){
						epati[i].x-=(player[j].x-epati[i].x)/sqrt((player[j].x-epati[i].x)*(player[j].x-epati[i].x)+(player[j].y-epati[i].y)*(player[j].y-epati[i].y))*alus.easevauhti;
						epati[i].y-=(player[j].y-epati[i].y)/sqrt((player[j].x-epati[i].x)*(player[j].x-epati[i].x)+(player[j].y-epati[i].y)*(player[j].y-epati[i].y))*alus.easevauhti;
					}else if(etaisyys>player[j].alus.sade+1.5f){
						epati[i].x+=(player[j].x-epati[i].x)/sqrt((player[j].x-epati[i].x)*(player[j].x-epati[i].x)+(player[j].y-epati[i].y)*(player[j].y-epati[i].y))*alus.easevauhti;
						epati[i].y+=(player[j].y-epati[i].y)/sqrt((player[j].x-epati[i].x)*(player[j].x-epati[i].x)+(player[j].y-epati[i].y)*(player[j].y-epati[i].y))*alus.easevauhti;
					}
					else if(etaisyys<player[j].alus.sade+1.5f){
						if(epati[i].ampumisviive<=0){
							epati[i].ampumisviive=30;
							if(!(player[j].alus.easelaji==0 && player[j].suojapaalla))
								if(efektitpaalla) eosumisaani->play();
								if(!joinpeli) if(!(player[j].alus.easelaji==0 && player[j].suojapaalla))player[j].energia-=alus.edamage;
//								if(player[j].energia<0) player[j].energia=0;
							int i2;
							for(i2=0;i2<maxviivat;i2++){
								if(viivat[i2].aika<=0){
									viivat[i2].x1=epati[i].x;
									viivat[i2].y1=epati[i].y;
									viivat[i2].x2=player[j].x;
									viivat[i2].y2=player[j].y;
									viivat[i2].r=1.0f;
									viivat[i2].g=1.0f;
									viivat[i2].xv=0;
									viivat[i2].yv=0;
									viivat[i2].aika=2;
									i2=maxviivat;
								}
							}
						}
					}
				}
			}
		}
		break;
	case 8:
		for (i=0;i<alus.maxepadit;i++){
			if(epati[i].aika>0){ 
				epati[i].aika--;
				if(epati[i].x>maxx)epati[i].xv-=0.02;
				if(epati[i].x<minx)epati[i].xv+=0.02;
				if(epati[i].y>maxy)epati[i].yv-=0.02;
				if(epati[i].y<miny)epati[i].yv+=0.02;
				epati[i].x+=epati[i].xv;
				epati[i].y+=epati[i].yv;
				epati[i].kulma=atan2(epati[i].yv,epati[i].xv)*180.0f/PI;
				double tempetaisyys=epati[i].x*epati[i].x+epati[i].y*epati[i].y;
				if(!joinpeli) if(tempetaisyys<planeettasadetoiseen) epati[i].aika=0;
				if(tempetaisyys<gravitysadetoiseen){
					epati[i].xv-=epati[i].x/(tempetaisyys)*patigravity;
					epati[i].yv-=epati[i].y/(tempetaisyys)*patigravity;
				}
				for(j=0;j<players;j++)if((!pistepeli) || player[j].pisteet>=0){
//					if(j!=numero)
					if(player[j].tormaa(epati[i], alus)){
						if(efektitpaalla) eosumisaani->play();
						if(!joinpeli) epati[i].aika=0;
						int scount=150+sets->detail*50;
						int j2=0;
						while(j2<sirpaleita && scount>0){
							if(sirpale[j2].aika<=0){
								scount--;
								sirpale[j2].x=player[j].x;
								sirpale[j2].y=player[j].y;
								sirpale[j2].xv=1;
								sirpale[j2].yv=1;
								sirpale[j2].zv=0;
								sirpale[j2].z=0;
								while(sqrt(sirpale[j2].xv*sirpale[j2].xv+sirpale[j2].yv*sirpale[j2].yv)>0.1f){
									sirpale[j2].xv=random(200)/1000.0f-0.1f;
									sirpale[j2].yv=random(200)/1000.0f-0.1f;
								}
								sirpale[j2].xv+=epati[i].xv;
								sirpale[j2].yv+=epati[i].yv;
								sirpale[j2].maxaika=75;
								sirpale[j2].aika=random(50)+25;
								sirpale[j2].koko=random(40)/100.0f;
								sirpale[j2].muoto=0;
								sirpale[j2].red=random(50)/40.0f+0.2f;
								sirpale[j2].green=sirpale[j2].red;
								sirpale[j2].blue=sirpale[j2].red;
							}
							j2++;
						}
						player[j].energia-=alus.edamage;
						player[j].xvauhti+=epati[i].xv*alus.epatipotkasu;
						player[j].yvauhti+=epati[i].yv*alus.epatipotkasu;
					}
				}
			}
		}
		break;
	case 9:
		for(i=0;i<alus.maxepadit;i++){
			if(epati[i].aika>0){
				epati[i].aika--;
				epati[i].x+=epati[i].xv;
				epati[i].y+=epati[i].yv;
				if(epati[i].x>maxx)epati[i].xv-=0.02;
				if(epati[i].x<minx)epati[i].xv+=0.02;
				if(epati[i].y>maxy)epati[i].yv-=0.02;
				if(epati[i].y<miny)epati[i].yv+=0.02;
				if(!joinpeli) if(epati[i].x*epati[i].x+epati[i].y*epati[i].y<planeettasadetoiseen){
					epati[i].aika=0;
				}
				for(j=0;j<players;j++) if(j!=numero)if((!pistepeli) || player[j].pisteet>=0){
					double xet=player[j].x-epati[i].x;
					double yet=player[j].y-epati[i].y;
					double etaisyys=sqrt(xet*xet+yet*yet);
					if(etaisyys<aukkogravitysade){
						player[j].xvauhti-=xet/(etaisyys*etaisyys)*aukkogravity*alus.paino/player[j].alus.paino;
						player[j].yvauhti-=yet/(etaisyys*etaisyys)*aukkogravity*alus.paino/player[j].alus.paino;
					}
					if(etaisyys<(alus.epatisade+player[j].alus.sade)*0.785){
						player[j].energia-=alus.edamage;
//						if(player[j].energia<0)player[j].energia=0;
					}
					for(k=0;k<player[j].alus.maxpadit;k++) if(player[j].pati[k].aika>0){
						xet=player[j].pati[k].x-epati[i].x;
						yet=player[j].pati[k].y-epati[i].y;
						etaisyys=sqrt(xet*xet+yet*yet);
						if(etaisyys<aukkogravitysade){
							player[j].pati[k].xv-=xet/(etaisyys*etaisyys)*aukkogravity*5;
							player[j].pati[k].yv-=yet/(etaisyys*etaisyys)*aukkogravity*5;
						}
						if(!joinpeli) if(etaisyys<(alus.epatisade+player[j].alus.patisade)*0.785){
							player[j].pati[k].energia-=alus.edamage;
							if(player[j].pati[k].energia<0){
								player[j].pati[k].energia=0;
								player[j].pati[k].aika=0;
							}
						}
					}
					for(k=0;k<player[j].alus.maxepadit;k++) if(player[j].epati[k].aika>0){
						xet=player[j].epati[k].x-epati[i].x;
						yet=player[j].epati[k].y-epati[i].y;
						etaisyys=sqrt(xet*xet+yet*yet);
						if(etaisyys<aukkogravitysade){
							player[j].epati[k].xv-=xet/(etaisyys*etaisyys)*aukkogravity*5;
							player[j].epati[k].yv-=yet/(etaisyys*etaisyys)*aukkogravity*5;
						}
						if(!joinpeli) if(etaisyys<(alus.epatisade+player[j].alus.epatisade)*0.785){
							player[j].epati[k].energia-=alus.edamage;
							if(player[j].epati[k].energia<0){
								player[j].epati[k].energia=0;
								player[j].epati[k].aika=0;
							}
						}
					}
				}

			}
		}
		break;
	case 13:
		for (i=0;i<alus.maxepadit;i++){
			if(epati[i].aika>0){ 
				epati[i].aika--;
				double tempetaisyys=epati[i].x*epati[i].x+epati[i].y*epati[i].y;
				if(!joinpeli) if(tempetaisyys<planeettasadetoiseen) epati[i].aika=0;
				double etaisyys=maxx*2+maxy*2;
				double xetaisyys;
				double yetaisyys;
				j=0;
				for(k=0;k<players;k++) if(k!=numero)if((!pistepeli) || player[k].pisteet>=0) if(player[k].tiimi!=tiimi){
					double xet=epati[i].x-player[k].x;
					double yet=epati[i].y-player[k].y;
					double etaisyys2=sqrt(xet*xet+yet*yet);
					if(k!=numero){
						if(etaisyys2<etaisyys){
							j=k;
							etaisyys=etaisyys2;
							xetaisyys=xet;
							yetaisyys=yet;
						}

					}
					if(etaisyys2<(alus.epatisade+player[k].alus.sade)*0.6f){
						epati[i].aika=0;
						player[k].lisaalima();
						if(efektitpaalla) eosumisaani->play();
					}
				}
				epati[i].x-=xetaisyys/etaisyys*alus.easevauhti;
				epati[i].y-=yetaisyys/etaisyys*alus.easevauhti;
			}
		}
		break;
	default:
		break;
	}
}


void Player::ease(){
	int i,k,count,scount,j2;
	switch(alus.numero){
	case 0:
		if(efektitpaalla) eampumisaani->play();
		if(!joinpeli) fuel-=alus.easefuel;
		easeviive=alus.easenopeus;
		easekesto=alus.easekesto;
		suojapaalla=true;
		break;
	case 1:
		easeviive=alus.easenopeus;
		if(efektitpaalla) eampumisaani->play();
		if(!joinpeli){
			fuel-=alus.easefuel;
			for(i=0;i<alus.maxepadit;i++){
				epati[i].x=x+cos((double)i/(double)alus.maxepadit*2*PI)*alus.sade*5;
				epati[i].y=y+sin((double)i/(double)alus.maxepadit*2*PI)*alus.sade*5;
				epati[i].xv=xvauhti+cos((double)i/(double)alus.maxepadit*2*PI)*alus.easevauhti;
				epati[i].yv=yvauhti+sin((double)i/(double)alus.maxepadit*2*PI)*alus.easevauhti;
				epati[i].aika=alus.easekesto;
				epati[i].energia=alus.epatienergia;
				epati[i].kulma=0;
			}
		}
		break;
	case 2:
		easeviive=alus.easenopeus;
//		if(efektitpaalla) eampumisaani->play();
		if(!joinpeli) energia=0;
		if(!joinpeli) fuel-=alus.easefuel;
		if(!joinpeli) for(i=0;i<players;i++){
			if(i!=numero){
				double xet=player[i].x-x;
				double yet=player[i].y-y;
				double etaisyys=sqrt(xet*xet+yet*yet);
				if(etaisyys<20 && !(player[i].alus.easelaji==0 && player[i].suojapaalla)) if((!pistepeli) || player[i].pisteet>=0)player[i].energia-=alus.edamage-etaisyys*4.5f;
//				if(player[i].energia<0)player[i].energia=0;
			}
			if(player[i].alus.patiVoiHajota)if((!pistepeli) || player[i].pisteet>=0){
				int j2;
				for(j2=0;j2<player[i].alus.maxpadit;j2++){
					if(player[i].pati[j2].aika>0){
						double xet=player[i].pati[j2].x-x;
						double yet=player[i].pati[j2].y-y;
						double etaisyys=sqrt(xet*xet+yet*yet);
						if(etaisyys<12){
							player[i].pati[j2].energia-=alus.edamage-etaisyys*6.0f;
							if(player[i].pati[j2].energia<=0) player[i].pati[j2].aika=0;
						}
					}
				}
			}
			if(player[i].alus.epatiVoiHajota)if((!pistepeli) || player[i].pisteet>=0){
				int j2;
				for(j2=0;j2<player[i].alus.maxepadit;j2++){
					if(player[i].epati[j2].aika>0){
						double xet=player[i].epati[j2].x-x;
						double yet=player[i].epati[j2].y-y;
						double etaisyys=sqrt(xet*xet+yet*yet);
						if(etaisyys<12){
							player[i].epati[j2].energia-=alus.edamage-etaisyys*6.0f;
							if(player[i].epati[j2].energia<=0) player[i].epati[j2].aika=0;
						}
					}
				}
			}
		}
		break;
	case 3:
		if(!joinpeli) fuel-=alus.easefuel;
		easeviive=alus.easenopeus;
		if(efektitpaalla) eampumisaani->play();
		if(!joinpeli) for(i=0;i<alus.maxepadit;i++){
			if(epati[i].aika<=0){
				epati[i].x=x+cos((kulma+180.0f)/180*PI)*alus.sade*1.1f;
				epati[i].y=y+sin((kulma+180.0f)/180*PI)*alus.sade*1.1f;
				epati[i].kulma=kulma-180.0f;
				epati[i].aika=alus.easekesto;
				epati[i].energia=alus.epatienergia;
				epati[i].kulma=0;
				i=alus.maxepadit;
			}
		}
		break;
	case 4:
		if(efektitpaalla) eampumisaani->play();
		if(!joinpeli) for(i=0;i<alus.maxepadit;i++){
			if(epati[i].aika<=0){
				epati[i].x=x+cos((kulma+180.0f)/180*PI)*alus.sade*1.1f;
				epati[i].y=y+sin((kulma+180.0f)/180*PI)*alus.sade*1.1f;
				epati[i].xv=xvauhti;
				epati[i].yv=yvauhti;
				epati[i].kulma=kulma-180.0f;
				epati[i].aika=alus.easekesto;
				epati[i].energia=alus.epatienergia;
				i=alus.maxepadit;
				fuel-=alus.easefuel;
				easeviive=alus.easenopeus;
			}
		}
		break;
	case 5:
		int i,j;
		if(efektitpaalla) eampumisaani->play();
		if(!joinpeli) for(j=0;j<2;j++){
			for(i=0;i<alus.maxepadit;i++){
				if(epati[i].aika<=0 && energia>2-j){
					energia--;
					epati[i].kulma=0;
					epati[i].energia=alus.epatienergia;
					epati[i].x=x+cos((kulma+160.0f+40.0f*(double)j)/180*PI)*alus.sade*1.1f;
					epati[i].y=y+sin((kulma+160.0f+40.0f*(double)j)/180*PI)*alus.sade*1.1f;
					epati[i].aika=alus.easekesto;
					fuel-=alus.easefuel/2.0f;
					easeviive=alus.easenopeus;
					i=alus.maxepadit;
				}
			}
		}
		break;
	case 6:
		if(efektitpaalla) eampumisaani->play();
		easeviive=alus.easenopeus;
		if(!joinpeli){
			teleportattu=true;
			uusx=0;
			uusy=0;
			fuel-=alus.easefuel;
			while(sqrt(uusx*uusx+uusy*uusy)<20.0f){
				uusx=random(maxx*2)-maxx;
				uusy=random(maxy*2)-maxy;	
			}
		}
		break;
	case 7:
		k=0;
		for(i=0;i<players;i++) if(i!=numero)if((!pistepeli) || player[i].pisteet>=0) if(player[i].tiimi!=tiimi){
			double xet=player[i].x-x;
			double yet=player[i].y-y;
			double etaisyys2=sqrt(xet*xet+yet*yet);
			if(etaisyys2<alus.epatisade+player[i].alus.sade*0.785){
				if(!joinpeli) if(!(player[i].alus.easelaji==0 && player[i].suojapaalla))player[i].energia-=alus.edamage;
//				if(player[i].energia<0) player[i].energia=0;
				for(k=0;k<maxviivat;k++){
					if(viivat[k].aika<=0){
						viivat[k].x1=player[i].x;
						viivat[k].y1=player[i].y;
						viivat[k].x2=x;
						viivat[k].y2=y;
						viivat[k].aika=alus.easekesto;
						viivat[k].r=0;
						viivat[k].g=1;
						viivat[k].b=1;
						k=maxviivat;
					}
				}
			}
			if(player[i].alus.patiVoiHajota) for(j=0;j<player[i].alus.maxpadit;j++){
				if(player[i].pati[j].aika>0){
					double xet=player[i].pati[j].x-x;
					double yet=player[i].pati[j].y-y;
					double etaisyys2=sqrt(xet*xet+yet*yet);
					if(etaisyys2<alus.epatisade+player[i].alus.patisade*0.785){
						if(!joinpeli) player[i].pati[j].energia-=alus.edamage;
						if(!joinpeli) if(player[i].pati[j].energia<0){
							player[i].pati[j].energia=0;
							player[i].pati[j].aika=0;
						}
						for(k=0;k<maxviivat;k++){
							if(viivat[k].aika<=0){
								viivat[k].x1=player[i].pati[j].x;
								viivat[k].y1=player[i].pati[j].y;
								viivat[k].x2=x;
								viivat[k].y2=y;
								viivat[k].aika=alus.easekesto;
								viivat[k].r=0;
								viivat[k].g=1;
								viivat[k].b=1;
								k=maxviivat;
							}
						}
					}
				}
			}
			if(player[i].alus.epatiVoiHajota) for(j=0;j<player[i].alus.maxepadit;j++){
				if(player[i].epati[j].aika>0){
					double xet=player[i].epati[j].x-x;
					double yet=player[i].epati[j].y-y;
					double etaisyys2=sqrt(xet*xet+yet*yet);
					if(etaisyys2<alus.epatisade+player[i].alus.epatisade*0.785){
						if(!joinpeli) player[i].epati[j].energia-=alus.edamage;
						if(!joinpeli) if(player[i].epati[j].energia<0){
							player[i].epati[j].energia=0;
							player[i].epati[j].aika=0;
						}
						for(k=0;k<maxviivat;k++){
							if(viivat[k].aika<=0){
								viivat[k].x1=player[i].epati[j].x;
								viivat[k].y1=player[i].epati[j].y;
								viivat[k].x2=x;
								viivat[k].y2=y;
								viivat[k].aika=alus.easekesto;
								viivat[k].r=0;
								viivat[k].g=1;
								viivat[k].b=1;
								k=maxviivat;
							}
						}
					}
				}
			}
		}
		if(k>0){
			fuel-=alus.easefuel;
			easeviive=alus.easenopeus;
			if(efektitpaalla) eampumisaani->play();
		}
		break;
	case 8:
		if(!joinpeli) fuel-=alus.easefuel;
		easeviive=alus.easenopeus;
		if(efektitpaalla) eampumisaani->play();
		if(!joinpeli) for(i=0;i<alus.maxepadit;i++){
			if(epati[i].aika<=0){
				epati[i].x=x+cos((kulma+180.0f)/180*PI)*alus.sade*1.1f;
				epati[i].y=y+sin((kulma+180.0f)/180*PI)*alus.sade*1.1f;
				epati[i].xv=cos((kulma+180.0f)/180*PI)*alus.easevauhti+xvauhti;
				epati[i].yv=sin((kulma+180.0f)/180*PI)*alus.easevauhti+yvauhti;
				epati[i].aika=alus.easekesto;
				epati[i].kulma=0;
				epati[i].energia=alus.epatienergia;
				i=alus.maxepadit;
			}
		}
		break;
	case 9:
		if(!joinpeli) fuel-=alus.easefuel;
		easeviive=alus.easenopeus;
		if(efektitpaalla) eampumisaani->play();
		if(!joinpeli) for(i=0;i<alus.maxepadit;i++){
			if(epati[i].aika<=0){
				epati[i].x=x+cos((kulma+180.0f)/180*PI)*alus.sade*1.1f;
				epati[i].y=y+sin((kulma+180.0f)/180*PI)*alus.sade*1.1f;
				epati[i].xv=cos((kulma+180.0f)/180*PI)*alus.easevauhti;
				epati[i].yv=sin((kulma+180.0f)/180*PI)*alus.easevauhti;
				epati[i].aika=alus.easekesto;
				epati[i].kulma=0;
				epati[i].energia=alus.epatienergia;
				i=alus.maxepadit;
			}
		}
		break;
	case 10:
		if(!joinpeli) fuel-=alus.easefuel;
		easeviive=alus.easenopeus;
		if(efektitpaalla) eampumisaani->play();
		int tsemit;
		if(!joinpeli) for(j=0;j<players;j++) if(j!=numero)if((!pistepeli) || player[j].pisteet>=0) if(player[j].tiimi!=tiimi){
			double xet=player[j].x-x;
			double yet=player[j].y-y;
			double etaisyys=sqrt(xet*xet+yet*yet);
			tsemit=0;
			if(etaisyys<23)tsemit++;
			if(etaisyys<19)tsemit++;
			if(etaisyys<15)tsemit++;
			if(etaisyys<11)tsemit++;
			for(k=0;k<tsemit;k++){
				if(random(100)<75 && player[j].energia>1){
					player[j].energia-=1;
					for(i=0;i<pikkumiehia;i++){
						if(pikkumies[i].aika<=0){
							pikkumies[i].x=player[j].x+random(300)/100.0f*player[j].alus.sade/2.0f-0.625*player[j].alus.sade;
							pikkumies[i].y=player[j].y+random(300)/100.0f*player[j].alus.sade/2.0f-0.625*player[j].alus.sade;
							pikkumies[i].xv=0;
							pikkumies[i].yv=0;
							pikkumies[i].aika=alus.easekesto;
							pikkumies[i].energia=alus.epatienergia;
							pikkumies[i].ampumisviive=70;
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
							i=pikkumiehia;
						}
					}
				}
			}
		}
/*		if(!joinpeli) for(i=0;i<alus.maxepadit;i++){
			if(epati[i].aika<=0){
				epati[i].x=x+cos((kulma+180.0f)/180*PI)*alus.sade*1.1f;
				epati[i].y=y+sin((kulma+180.0f)/180*PI)*alus.sade*1.1f;
				epati[i].xv=cos((kulma+180.0f)/180*PI)*alus.easevauhti;
				epati[i].yv=sin((kulma+180.0f)/180*PI)*alus.easevauhti;
				epati[i].aika=alus.easekesto;
				epati[i].energia=alus.epatienergia;
				epati[i].kulma=0;
				i=alus.maxepadit;
			}
		}*/
		break;
	case 11:
		if(!joinpeli) fuel-=alus.easefuel;
		easeviive=alus.easenopeus;
		if(efektitpaalla) eosumisaani->play();
		if(suojapaalla) suojapaalla=false;
		else suojapaalla=true;
		break;
	case 12:
		if(efektitpaalla) eampumisaani->play();
		if(!joinpeli) fuel+=alus.fuel/3;
		if(fuel>alus.fuel)fuel=alus.fuel;
		easeviive=alus.easenopeus;
		pease=false;
		break;
	case 13:
		easeviive=alus.easenopeus;
		if(efektitpaalla) eampumisaani->play();
		if(!joinpeli) for(i=0;i<alus.maxepadit;i++){
			if(epati[i].aika<=0){
				fuel-=alus.easefuel;
				epati[i].x=x+cos((kulma+180.0f)/180*PI)*alus.sade*1.1f;
				epati[i].y=y+sin((kulma+180.0f)/180*PI)*alus.sade*1.1f;
				epati[i].aika=alus.easekesto;
				epati[i].energia=alus.epatienergia;
				epati[i].kulma=0;
				i=alus.maxepadit;
			}
		}
		break;
	case 14:
		easeviive=alus.easenopeus;
		if(!joinpeli) fuel-=alus.easefuel;
		if(efektitpaalla) eampumisaani->play();
		if(!joinpeli) yvauhti+=sin(kulma/180.0*PI)*alus.easevauhti;
		if(!joinpeli) xvauhti+=cos(kulma/180.0*PI)*alus.easevauhti;
		scount=50;
		j2=0;
		while(j2<sirpaleita && scount>0){
			if(sirpale[j2].aika<=0){
				scount--;
				sirpale[j2].x=x-cos(kulma);
				sirpale[j2].y=y-sin(kulma);
				sirpale[j2].xv=1;
				sirpale[j2].yv=1;
				sirpale[j2].zv=0;
				sirpale[j2].z=0;
				while(sirpale[j2].xv*sirpale[j2].xv+sirpale[j2].yv*sirpale[j2].yv>0.015f){
					sirpale[j2].xv=random(25)/50.0f-0.25f;
					sirpale[j2].yv=random(25)/50.0f-0.25f;
				}
				sirpale[j2].xv+=cos(kulma/180.0f*PI-PI)*(random(100)/100.0f+0.5)+xvauhti;
				sirpale[j2].yv+=sin(kulma/180.0f*PI-PI)*(random(100)/100.0f+0.5)+yvauhti;
				sirpale[j2].maxaika=100;
				sirpale[j2].aika=random(100)+50;
				sirpale[j2].koko=random(20)/100.0f+0.2;
				sirpale[j2].muoto=0;
				sirpale[j2].red=random(40)/100.0f+0.6f;
				sirpale[j2].green=random(20)/100.0f;
				sirpale[j2].blue=random(20)/100.0f;
			}
			j2++;
		}
		break;
	case 15:
		if(!suojapaalla){
			if(efektitpaalla) eampumisaani->play();
			if(!joinpeli) fuel-=alus.easefuel;
			easeviive=alus.easenopeus;
			suojapaalla=true;
		}
		break;
	case 16:
		if(!joinpeli) fuel-=alus.easefuel;
		easeviive=alus.easenopeus;
		if(efektitpaalla) eampumisaani->play();
		if(!joinpeli) for(i=0;i<alus.maxepadit;i++){
			if(epati[i].aika<=0){
				epati[i].x=x+cos((kulma+180.0f)/180*PI)*alus.sade*1.1f;
				epati[i].y=y+sin((kulma+180.0f)/180*PI)*alus.sade*1.1f;
				epati[i].xv=cos((kulma+180.0f)/180*PI)*alus.easevauhti;
				epati[i].yv=sin((kulma+180.0f)/180*PI)*alus.easevauhti;
				epati[i].aika=alus.easekesto;
				epati[i].r=1;
				epati[i].g=0;
				epati[i].b=0;
				epati[i].kulma=0;
				epati[i].energia=alus.epatienergia;
				i=alus.maxepadit;
			}
		}
		break;
	case 17:
		if(!joinpeli) fuel-=alus.easefuel;
		easeviive=alus.easenopeus;
		if(efektitpaalla) eampumisaani->play();
		if(!joinpeli) for(i=0;i<alus.maxepadit;i++){
			if(epati[i].aika<=0){
				epati[i].x=x+cos((kulma+180.0f)/180*PI)*alus.sade*1.1f;
				epati[i].y=y+sin((kulma+180.0f)/180*PI)*alus.sade*1.1f;
				epati[i].xv=0;
				epati[i].yv=0;
				epati[i].aika=alus.easekesto;
				epati[i].kulma=0;
				epati[i].energia=alus.epatienergia;
				i=alus.maxepadit;
			}
		}
		if(!joinpeli) x+=cos(kulma/180.0f*PI)*alus.easevauhti;
		if(!joinpeli) y+=sin(kulma/180.0f*PI)*alus.easevauhti;
		if(!joinpeli) if(sqrt(xvauhti*xvauhti+yvauhti*yvauhti)<0.4){
			xvauhti+=cos(kulma/180.0f*PI)*alus.easevauhti/5.0f;
			yvauhti+=sin(kulma/180.0f*PI)*alus.easevauhti/5.0f;
		}
		if(!joinpeli) xvauhti+=cos(kulma/180.0f*PI)*alus.easevauhti/30.0f;
		if(!joinpeli) yvauhti+=sin(kulma/180.0f*PI)*alus.easevauhti/30.0f;
		break;
	case 18:
		for(i=0;i<asteroideja;i++){
			double xet=x-asteroidi[i].x;
			double yet=y-asteroidi[i].y;
			double etaisyys=sqrt(xet*xet+yet*yet);
			if(etaisyys<alus.epatisade){
				if(efektitpaalla) eampumisaani->play();
				easeviive=alus.easenopeus;
				if(!joinpeli) asteroidi[i].energia=0;
				if(!joinpeli) fuel=alus.fuel;
			}
		}
		break;
	case 19:
		if(!joinpeli) fuel-=alus.easefuel;
		easeviive=alus.easenopeus;
		if(efektitpaalla) eampumisaani->play();
		ampumispuoli=2;
		break;
	case 20:
		if(!joinpeli) fuel-=alus.easefuel;
		easeviive=alus.easenopeus;
		ampumispuoli=3;
		break;
	case 21:
		if(!joinpeli) fuel-=alus.easefuel;
		easeviive=alus.easenopeus;
		if(efektitpaalla) eampumisaani->play();
		if(!joinpeli) for(i=0;i<alus.maxepadit;i++){
			if(epati[i].aika<=0){
				epati[i].x=x+cos((kulma+180.0f)/180*PI)*alus.sade*1.3f;
				epati[i].y=y+sin((kulma+180.0f)/180*PI)*alus.sade*1.3f;
				epati[i].xv=cos((kulma+180.0f)/180*PI)*alus.easevauhti+xvauhti;
				epati[i].yv=sin((kulma+180.0f)/180*PI)*alus.easevauhti+yvauhti;
				epati[i].aika=alus.easekesto;
				epati[i].kulma=0;
				epati[i].energia=alus.epatienergia;
				epati[i].r=1;
				epati[i].g=0;
				i=alus.maxepadit;
			}
		}
		break;
	case 22:
		if(!joinpeli) fuel-=alus.easefuel;
		easeviive=alus.easenopeus;
		if(efektitpaalla) eampumisaani->play();
		count=25;
		if(!joinpeli) for(i=0;i<alus.maxepadit;i++){
			if(epati[i].aika<=0){
				epati[i].aika=alus.easekesto;
				epati[i].energia=alus.epatienergia;
				epati[i].x=xhanta[palikkamaara-1]+cos(kulmahanta[palikkamaara-1]*PI/180.0f-PI)*2.0f;
				epati[i].y=yhanta[palikkamaara-1]+sin(kulmahanta[palikkamaara-1]*PI/180.0f-PI)*2.0f;
				epati[i].xv=cos(kulmahanta[palikkamaara-1]*PI/180.0f-PI)*(alus.easevauhti+random(100)/200.0f);
				epati[i].yv=sin(kulmahanta[palikkamaara-1]*PI/180.0f-PI)*(alus.easevauhti+random(100)/200.0f);
				count --;
				if(count<=0) break;

			}
		}
		break;
	case 23:
		if(!joinpeli) fuel-=alus.easefuel;
		easeviive=alus.easenopeus;
		if(efektitpaalla) eampumisaani->play();
		if(!joinpeli) for(i=0;i<alus.maxepadit;i++){
			if(epati[i].aika<=0){
				epati[i].aika=alus.easekesto;
				epati[i].energia=alus.epatienergia;
				epati[i].x=x;
				epati[i].y=y;
				epati[i].xv=0;
				epati[i].yv=0;
				break;
			}
		}
		break;

	default:

		break;
	}
}


void Player::ailiiku(){
	//alustukset
	double kohdeetaisyys=900000000;
	double kohdeetaisyys2;
	int kohde=-1;
	int kohde2;
	for(kohde2=0;kohde2<players;kohde2++) if(kohde2!=numero){
		if((!pistepeli || player[kohde2].pisteet>=0) && player[kohde2].kuolema<20){
			kohdeetaisyys2=(player[kohde2].x-x)*(player[kohde2].x-x)+(player[kohde2].y-y)*(player[kohde2].y-y);
			if(kohdeetaisyys2<kohdeetaisyys && (player[kohde2].tiimi!=tiimi)){
				kohdeetaisyys=kohdeetaisyys2;
				kohde=kohde2;
			}
		}
	}
	if(kohde<0 && !sets->aikohteet){
		for(kohde2=0;kohde2<players;kohde2++) if(kohde2!=numero){
			if((!pistepeli) || player[kohde2].pisteet>=0){
				kohdeetaisyys2=(player[kohde2].x-x)*(player[kohde2].x-x)+(player[kohde2].y-y)*(player[kohde2].y-y);
				if(kohdeetaisyys2<kohdeetaisyys){
					kohdeetaisyys=kohdeetaisyys2;
					kohde=kohde2;
				}
			}
		}
	}
	//else if(kohde<0 && numero>0)kohde=0;
	//else if(kohde<0) kohde=1;
	double xet=0,yet=0;
	if(kohde>=0){
		xet=player[kohde].x-x;
		yet=player[kohde].y-y;
	}
	double kulma2=atan2(yet,xet);
	double etaisyys=sqrt(xet*xet+yet*yet);
	double ennakkokulma;
	double ennakkox, ennakkoy,ennakkoetaisyys;
	double okulma=kulma/180.0f*PI;
	double lvalittu=false, rvalittu=false;

	//ennakkokulma
	if(kohde>=0){
		if(alus.patinopeus>0.05 && alus.patinopeus<4){
			ennakkox=player[kohde].xvauhti*etaisyys/(alus.patinopeus+1.0*xvauhti)+xet;
			ennakkoy=player[kohde].yvauhti*etaisyys/(alus.patinopeus+1.0*yvauhti)+yet;
			ennakkoetaisyys=sqrt(ennakkoy*ennakkoy+ennakkox*ennakkox);
		}else if(alus.patinopeus<0.05){
			ennakkox=player[kohde].xvauhti*sqrt(etaisyys/(0.5*alus.patinopeus));
			ennakkoy=player[kohde].yvauhti*sqrt(etaisyys/(0.5*alus.patinopeus));
			ennakkoetaisyys=sqrt(ennakkoy*ennakkoy+ennakkox*ennakkox);
		}
		if(alus.patinopeus>0.05 && alus.patinopeus<4){
			if(ennakkoetaisyys>alus.patinopeus*alus.patiaika){
				ennakkokulma=kulma2;
			}else{
				ennakkokulma=atan2(yet+player[kohde].yvauhti*ennakkoetaisyys/(alus.patinopeus+1.0*yvauhti),
				               xet+player[kohde].xvauhti*ennakkoetaisyys/(alus.patinopeus+1.0*xvauhti));
			}
		}else if(alus.patinopeus<0.05 && alus.patisade<=2){
			ennakkokulma=atan2(yet+player[kohde].yvauhti*sqrt(ennakkoetaisyys/(0.5*alus.patinopeus)),
				               xet+player[kohde].xvauhti*sqrt(ennakkoetaisyys/(0.5*alus.patinopeus)));
		}else{
			ennakkokulma=kulma2;
		}
	}


	//katsotaan painetaanko kaasua
	if(kohde>=0){
		if(alus.aselaji==0  || alus.numero==16){
			if(fabs(kulma2*180.0f/PI-kulma)<45 && sqrt(xet*xet+yet*yet)>alus.patiaika*alus.patinopeus/3*2) pylos=true;
				else pylos=false;
		}else if(alus.aselaji==4){
			if(fabs(kulma2*180.0f/PI-kulma)<45 && 
				sqrt(xet*xet+yet*yet)>player[kohde].alus.patiaika*player[kohde].alus.patinopeus*1.2) pylos=true;
					else pylos=false;
		}else if(alus.aselaji==8 || alus.numero==21){
			pylos=true;
		}else if(alus.easelaji==10){
			int i;
			for(i=0;i<pikkumiehia;i++){
				if(pikkumies[i].aika>0){
					pylos=true;
					double xet2=pikkumies[i].x-x;
					double yet2=pikkumies[i].y-y;
					double kulma3=atan2(yet2,xet2);
					double kulma4=atan2(yvauhti,xvauhti)*180.0f/PI;
					lvalittu=true;
					rvalittu=true;
					if(sqrt(xvauhti*xvauhti+yvauhti*yvauhti)>sqrt(xet2*xet2+yet2*yet2)/10){
						if((kulma3*180.0f/PI>kulma4 && kulma3*180.0f/PI<kulma4+180) || kulma3*180.0f/PI<kulma4-180){
							pleft=true;
							pright=false;
						}else {
							pright=true;
							pleft=false;
						}
					}else{
						if((kulma3*180.0f/PI>kulma && kulma3*180.0f/PI<kulma+180) || kulma3*180.0f/PI<kulma-180){
							pleft=true;
							pright=false;
						}else {
							pright=true;
							pleft=false;
						}
					}			

				}
			}
			if(!lvalittu){
				if(fabs(kulma2*180.0f/PI-kulma)<45 && sqrt(xet*xet+yet*yet)>alus.patiaika*alus.patinopeus/3*2) pylos=true;		
				 else pylos=false;
			}
		}
		else if(alus.easelaji==14){
			if((sqrt(xet*xet+yet*yet)<player[kohde].alus.patiaika*player[kohde].alus.patinopeus || sqrt(xet*xet+yet*yet)<15) 
				&& fabs(kulma2*180.0f/PI-kulma)<10) pylos=true;
			else pylos=false;
		}
		else{
			if(fabs(kulma2*180.0f/PI-kulma)<90) pylos=true;
			else pylos=false;
		}
		if(alus.numero==18) pylos=false;
	}

	//normaali kääntyminen
	if(kohde>=0){
		if(!lvalittu && !rvalittu){
			if(alus.numero==22){
				//haulikko kehiin
				if(etaisyys<50 || (fabs(okulma-ennakkokulma)>PI/2 && etaisyys<75)){
					if((okulma>ennakkokulma && okulma<ennakkokulma+PI) || okulma<ennakkokulma-PI){
						pleft=true;
						pright=false;
					}else{
						pleft=false;
						pright=true;
					}
				}else{//joku muu ase kehiin
					if((okulma>ennakkokulma && okulma<ennakkokulma+PI) || okulma<ennakkokulma-PI){
						pleft=false;
						pright=true;
					}else{
						pleft=true;
						pright=false;
					}
				}
			}
			else if(alus.numero==18){
				if(fuel>=alus.patifuel){
					if((kulma2>okulma && kulma2<okulma+PI) || kulma2<okulma-PI){
						pleft=true;
						pright=false;
					}else{
						pleft=false;
						pright=true;
					}
				}else{
					double aetaisyys=10000;
					double axet=0;
					double ayet=0;
					int k=0;
					int i;
					for(i=0;i<asteroideja;i++){
						double axet2=asteroidi[i].x-x;
						double ayet2=asteroidi[i].y-y;
						double a2etaisyys=sqrt(axet2*axet2+ayet2*ayet2);
						if(a2etaisyys<aetaisyys &&
							asteroidi[i].x<maxx-10 &&
							asteroidi[i].x>minx+10 &&
							asteroidi[i].y<maxy-10 &&
							asteroidi[i].y>miny+10){
							axet=axet2;
							ayet=ayet2;
							aetaisyys=a2etaisyys;
							k=i;
						}
					}
					double kulman=atan2(ayet,axet);
					if((kulman>okulma && kulman<okulma+PI) || kulman<okulma-PI){
						pleft=true;
						pright=false;
					}else{
						pleft=false;
						pright=true;
					}
				}
			}else if((alus.aselaji==12 && etaisyys>alus.patiaika*alus.patinopeus) || alus.aselaji==6 
				|| alus.numero==18 || alus.aselaji==9
				|| alus.aselaji==15 || alus.easelaji==14){// || alus.numero==10){
				if((kulma2*180.0f/PI>kulma && kulma2*180.0f/PI<kulma+180) || kulma2*180.0f/PI<kulma-180){
					pleft=true;
					pright=false;
				}else {
					pright=true;
					pleft=false;
				}		

			}else if((alus.aselaji!=8 && alus.numero!=21) || fabs(kulma2*180.0f/PI-kulma)<90 || etaisyys>40){
				if((ennakkokulma*180.0f/PI>kulma && ennakkokulma*180.0f/PI<kulma+180) || ennakkokulma*180.0f/PI<kulma-180){
					pleft=true;
					pright=false;
				}else {
					pright=true;
					pleft=false;
				}
			}else{
				if((ennakkokulma*180.0f/PI>kulma && ennakkokulma*180.0f/PI<kulma+180) || ennakkokulma*180.0f/PI<kulma-180){
					pleft=false;
					pright=true;
				}else {
					pright=false;
					pleft=true;
				}
			}
		}
	}

	//planeetta
	double peetaisyys=35;
	peetaisyys+=10-alus.paino/50.0f;
	if(alus.aselaji==6 || alus.aselaji==18 || alus.numero==22) peetaisyys=35;
	if(alus.aselaji==1) peetaisyys=45;
	if(alus.aselaji==0) peetaisyys=45;
	if(alus.aselaji==15) peetaisyys=40;
	if(alus.aselaji==17 || alus.aselaji==2) peetaisyys=50;

	double vkulma=atan2(yvauhti, xvauhti);
	double pkulma=atan2(-1*y,-1*x);
	if(pkulma-vkulma>PI) vkulma+=2*PI;
	if(pkulma-vkulma<-PI) vkulma-=2*PI;
	if(pkulma-okulma>PI) okulma+=2*PI;
	if(pkulma-okulma<-PI) okulma-=2*PI;

	double xvgravity=xvauhti+x/(pow(sqrt(x*x+y*y),2.3))*gravity*(pmassa/alus.paino);
	double yvgravity=yvauhti+y/(pow(sqrt(x*x+y*y),2.3))*gravity*(pmassa/alus.paino);

	//planeetta takana
	if(sqrt(x*x+y*y)<peetaisyys)
		pylos=true;
	double easevalittu=false;
	if(sqrt(x*x+y*y)<peetaisyys/2 && fabs(okulma-pkulma)>PI/2){
		if(fabs(okulma-vkulma)<PI/20 && fabs(atan2(yvgravity,xvgravity)-pkulma)>PI/2){
			pleft=false;
			pright=false;
		}
		else if(fabs(atan2(yvgravity,xvgravity)-pkulma)<PI/2 && (alus.numero==14 || alus.numero==15 || alus.numero==17)){
			pease=true;
			easevalittu=true;
		}
		else if(okulma-vkulma<0 && fabs(atan2(yvgravity,xvgravity)-pkulma)<PI/2){
			pleft=true;
			pright=false;
		}else if(fabs(atan2(yvgravity,xvgravity)-pkulma)<PI/2){
			pleft=false;
			pright=true;
		}
	}

	//planeetta edessä
	if(sqrt(x*x+y*y)<peetaisyys && fabs(vkulma-pkulma)<PI/3){
		if(vkulma-pkulma>0){
//			if(kulma/180*PI-pkulma<PI/5 || kulma/180*PI-pkulma>2*PI-PI/5){
			if(fabs(vkulma-pkulma)<PI/4 && fabs(vkulma-okulma)<PI/5){//sqrt(x*x+y*y)/peetaisyys*PI){//PI/6
				pleft=true;
				pright=false;
			}else{
				pleft=false;
				pright=false;
			}

		}else{
//			if(kulma/180*PI-pkulma>-PI/5 || kulma/180*PI-pkulma<-2*PI+PI/5){
			if(fabs(vkulma-pkulma)<PI/4 && fabs(vkulma-okulma)<PI/5){//<sqrt(x*x+y*y)/peetaisyys*PI){//PI/6
				pright=true;
				pleft=false;
			}else{
				pleft=false;
				pright=false;
			}
		}
	}

	//ampuminen
	if(kohde>=0){
		xet=player[kohde].x-x;
		yet=player[kohde].y-y;
		kulma2=atan2(yet,xet);
		if(alus.aselaji==11 && suojapaalla){
			if(sqrt(xet*xet+yet*yet)<alus.patiaika*alus.patinopeus
				&& fabs(ennakkokulma*180.0f/PI-kulma)<30 
				&& fuel>=alus.easefuel+alus.patifuel) pfire=true;
			else pfire=false;
		}
		else if(alus.aselaji==0 || alus.aselaji==1 || alus.aselaji==2 || alus.aselaji==3 || alus.aselaji==10
			|| alus.aselaji==8 || alus.numero==16 ||alus.numero==21){
			if(sqrt(xet*xet+yet*yet)<alus.patiaika*alus.patinopeus
				&& fabs(ennakkokulma*180.0f/PI-kulma)<30) pfire=true;
			else pfire=false;
		}else if(alus.aselaji==6 || alus.aselaji==18){
			int i;
			pfire=false;
			if(player[kohde].alus.patiVoiHajota) for(i=0;i<player[kohde].alus.maxpadit;i++) if(player[kohde].pati[i].aika>0){
				double xet2=player[kohde].pati[i].x-x;
				double yet2=player[kohde].pati[i].y-y;
				if(sqrt(xet2*xet2+yet2*yet2)<alus.patisade) pfire=true;
			}
			if(player[kohde].alus.epatiVoiHajota) for(i=0;i<player[kohde].alus.maxepadit;i++) if(player[kohde].epati[i].aika>0){
				double xet2=player[kohde].epati[i].x-x;
				double yet2=player[kohde].epati[i].y-y;
				if(sqrt(xet2*xet2+yet2*yet2)<alus.patisade) pfire=true;
			}
			if(sqrt(xet*xet+yet*yet)<alus.patisade+player[kohde].alus.sade) pfire=true;
			if(sqrt(xet*xet+yet*yet)<player[kohde].alus.sade+1.5 && fuel<alus.easefuel+alus.patifuel)
				pfire=false;
		}
		else if(alus.aselaji==9){
			if(etaisyys<alus.patisade+player[kohde].alus.sade) pfire=true;
			else pfire=false;
			if(fabs(kulma2*180.0f/PI-kulma)>35) pfire=false;
		}else if(alus.easelaji==11 && !suojapaalla){
			if(sqrt(xet*xet+yet*yet)<alus.epatisade) pfire=true;
			else pfire=false;
			if(fabs(kulma2*180.0f/PI-kulma)>35) pfire=false;
		}else if(alus.aselaji==4){
			if(pati[0].aika<=0){
				if(fabs(ennakkokulma*180.0f/PI-kulma)<7){
					pfire=true;
				}else pfire=false;
			}else if(pati[0].aika>0){
				double kulma3=atan2(pati[0].yv, pati[0].xv);
				double kulma4=atan2(player[kohde].y-pati[0].y, player[kohde].x-pati[0].x);
				if(fabs(kulma3-kulma4)<PI/3.0f) pfire=true;
				else pfire=false;
			}
		}else if(alus.aselaji==13){
			if(pfire){
				if(fabs(ennakkokulma*180.0f/PI-kulma)<3.0f
					&& sqrt(xet*xet+yet*yet)<alus.patiaika*alus.patinopeus*4/5
					&& pati[0].energia>250)
					pfire=false;	

				else pfire=true;
			}else{
				pfire=true;
			}
		}else if(alus.aselaji==14){
			if(fabs(kulma2*180.0f/PI-kulma)<10.0f)pfire=true;
			else pfire=false;	

		}else if(alus.aselaji==15){
			if(fuel>=alus.fuel-1) pfire=true;
			else if(sqrt(xet*xet+yet*yet)<40) pfire=true;
			else pfire=false;
		}else if(alus.numero==17){
			if(sqrt(xet*xet+yet*yet)<alus.patiaika*alus.patinopeus && fabs(okulma-kulma2)<PI/4
				&& fuel>=alus.fuel-2) pfire=true;
			else pfire=false;
		}else if(alus.numero==19){
			if(sqrt(xet*xet+yet*yet)<alus.patiaika*alus.patinopeus && fabs(okulma-kulma2)<PI/4
				&& fuel>=alus.fuel/2) pfire=true;
			else pfire=false;
		}else if(alus.numero==20){
			if(etaisyys<alus.patiaika*alus.patinopeus && fabs(ennakkokulma-kulma2)<PI/4
				&& (fuel>=(alus.fuel/2.0f) || player[kohde].alus.energia>=30)) pfire=true;
			else pfire=false;
		}
		else if(alus.patisade>4){
			if(sqrt(xet*xet+yet*yet)<alus.patisade && fabs(ennakkokulma*180.0f/PI-kulma)<30) pfire=true;
			else pfire=false;
		}else if(alus.patinopeus<0.03){
			if(sqrt(xet*xet+yet*yet)<0.5*alus.patiaika*alus.patiaika*alus.patinopeus && pylos) pfire=true;
			else pfire=false;
		}else{
			if(sqrt(xet*xet+yet*yet)<alus.patiaika*alus.patinopeus && pylos) pfire=true;
			else pfire=false;
		}	
	}
	if(kohde<0) pfire=false;
	//erikoisase


	if(kohde>=0){
	switch(alus.numero){
	case 0:
		int i;
		if(xet*xet+yet*yet<pow(player[kohde].alus.patisade,2)
			|| (player[kohde].alus.numero==14 && etaisyys<player[kohde].alus.sade+alus.sade+1)){
			pease=true;
		}else pease=false;
		if(pease==false){
			for(i=0;i<player[kohde].alus.maxpadit;i++){
				if(player[kohde].pati[i].aika>0){
					double pxet=player[kohde].pati[i].x-x;
					double pyet=player[kohde].pati[i].y-y;
					if(pxet*pxet+pyet*pyet<pow(alus.sade+player[kohde].alus.patisade+1,2)){
						pease=true;
						i=player[kohde].alus.maxpadit;
					}
				}
			}
		}
		if(pease==false){
			for(i=0;i<player[kohde].alus.maxepadit;i++){
				if(player[kohde].epati[i].aika>0){
					double pxet=player[kohde].epati[i].x-x;
					double pyet=player[kohde].epati[i].y-y;
					if(pxet*pxet+pyet*pyet<pow(alus.sade+player[kohde].alus.epatisade+2,2)){
						pease=true;
						i=player[kohde].alus.maxepadit;
					}
				}
			}
		}
		break;
	case 1:
		if(etaisyys<alus.easekesto*alus.easevauhti && !pfire && !pease) {
			pease=true;
		}else pease=false;
		break;
	case 2:
		if(sqrt(xet*xet+yet*yet)<3 && player[kohde].kuolema<=0)pease=true;
		else if(fabs(player[kohde].kulma-kulma2)>135.0/180.0*PI && sqrt(xet*xet+yet*yet)<7 && player[kohde].kuolema<=0)pease=true;
		else pease=false;
		if(player[kohde].alus.numero==0 && player[kohde].suojapaalla) pease=false;
		break;
	case 3:
		if(fabs(kulma2*180.0f/PI-kulma)>90 && 
			sqrt(xet*xet+yet*yet)<alus.patiaika*alus.patinopeus
			&& !pfire && !pease) pease=true;
		else pease=false;
		break;
	case 4:
		if(!pfire) pease=true;
		break;
	case 5:
		if(energia>alus.energia/2 && sqrt(xet*xet+yet*yet)>alus.patiaika*alus.patinopeus)pease=true;
		else pease=false;
		if(player[kohde].alus.easelaji==7) pease=false;
		break;
	case 6:
		if(sqrt(xet*xet+yet*yet)<(alus.sade+player[kohde].alus.sade)/4*PI+2.5) pease=true;
		else pease=false;
		break;
	case 7:
		pease=true;
		break;
	case 8:
		if(fabs(kulma2*180.0f/PI-kulma)>90 && !pfire && !pease)
			pease=true;
		else pease=false;
		break;
	case 9:
		if(sqrt(xet*xet+yet*yet)>alus.patisade && !pylos) pease=true;
		else pease=false;
		break;
	case 10:
		if(sqrt(xet*xet+yet*yet)<alus.epatisade) pease=true;
		else pease=false;
		break;
	case 11:
		if(suojapaalla && sqrt(xet*xet+yet*yet)<alus.epatisade+5) pease=true;
		else if(!suojapaalla && sqrt(xet*xet+yet*yet)>alus.epatisade+15) pease=true;
		else pease=false;
		break;
	case 12:
		if(fuel<alus.fuel)pease=true;
		else pease=false;
		break;
	case 13:
		if(player[kohde].limacount<maxlimat && sqrt(xet*xet+yet*yet)<alus.easevauhti*alus.easekesto) 
			pease=true;
		else pease=false;
		break;
	case 14:
		if(sqrt(xet*xet+yet*yet)<15
		&& fabs(kulma2*180.0f/PI-kulma)<10) pease=true;
		else pease=false;
		if(easevalittu) pease=true;
		break;
	case 15:
		if(!pease && sqrt(xet*xet+yet*yet)<30 && fuel<alus.fuel/3 && fabs(okulma-kulma2)<PI/6){
			pease=true;
		}else pease=false;
		if(easevalittu) pease=true;
		break;
	case 16:
		if(etaisyys<alus.easekesto*alus.easevauhti*1.5 && fabs(okulma-kulma2)>PI-PI/6) {
			pease=true;
		}else pease=false;
		break;
	case 17:
		if((etaisyys<15 && fabs(okulma-kulma2)>PI-PI/6)
			|| etaisyys<10) {
			pease=true;
		}else pease=false;
		if(easevalittu || fuel>=alus.fuel) pease=true;
		break;
	case 18:
		pease=true;
		break;
	case 19:
		if(etaisyys<(alus.sade+player[kohde].alus.sade)*0.785 && fabs(okulma-kulma2)<PI/4) pease=true;
		else pease=false;
		break;
	case 20:
		if(etaisyys<alus.easevauhti*alus.easekesto/1.75 && player[kohde].alus.energia<30) pease=true;
		else pease=false;
		break;
	case 21:
		if(fabs(kulma2*180.0f/PI-kulma)>90 && !pfire && !pease)
			pease=true;
		else pease=false;
		break;
	case 22:
		if(fabs(ennakkokulma-kulma2)<PI/2 && (etaisyys<75 || fuel>=alus.fuel/2)) pease=true;
		else pease=false;
		break;
	case 23:
		pease=false;
		break;
	default:
		break;
	}}
	if(kohde<0)pease=false;
//	}else pease=false;
}

void Player::liiku(){
	//if(joinase)printf("p%i jase=1\n",0);
	//else printf("p%i jase=0\n",0);

	if(alus.numero==22 && !joinpeli) pylos=true;
	if((!joinpeli && pease) || (joinpeli && joinease)){
		if(fuel>=alus.easefuel && easeviive<1){
			ease();
		}
	}
	if(easekesto>0)easekesto--;
	if(alus.aselaji==15 && suojapaalla){
		if(fuel<=0){
			suojapaalla=false;
			fuel=0;
		}else fuel-=alus.easefuel;
	}
	if(easekesto<=0 && alus.easelaji==0)suojapaalla=false;
	if(easeviive>0) easeviive--;
	if(!joinpeli){
		if(pfire && (alus.numero!=15 || !suojapaalla)){
			if(fuel>=alus.patifuel && ampumisviive<1) ammu();
		}
		if(!pfire && alus.aselaji==13 && pati[0].aika>0) ammu();
	}else{
		if(joinase && (alus.numero!=15 || !suojapaalla)){
			if(fuel>=alus.patifuel && ampumisviive<1) ammu();
		}
		if(!joinase && alus.aselaji==13 && pati[0].aika>0) ammu();
	}

	if(!joinpeli){
		if(!pfire || !(fuel>=alus.patifuel && ampumisviive<1)){
			while(asekulma>kulma+180.0f) asekulma-=360.0f;
			while(asekulma<kulma-180.0f) asekulma+=360.0f;
			if(asekulma>kulma) asekulma-=1.5f;
			if(asekulma<kulma) asekulma+=1.5f;
		}
	}else{
		if(!joinase || !(fuel>=alus.patifuel && ampumisviive<1)){
			while(asekulma>kulma+180.0f) asekulma-=360.0f;
			while(asekulma<kulma-180.0f) asekulma+=360.0f;
			if(asekulma>kulma) asekulma-=1.5f;
			if(asekulma<kulma) asekulma+=1.5f;
		}
	}

	double limakerroin=1.0f-(double)limacount/maxlimat*0.8;

	if(!joinpeli) if(alus.numero==18 && !pylos && ampumispuoli==0){
		ampumispuoli=1;
	}
	if(!joinpeli) if(alus.numero==18 && pylos && ampumispuoli==1){
		kulma=kulma+180;
		if(kulma>180) kulma-=360;
		ampumispuoli=0;
	}

	//hajoavista tulevia juttuja..
	if(sets->detail>0){
		if(kuolema<1 && energia<alus.energia){
			if(random(15000)/100.0f*energia<alus.energia){
				int j=0, scount=1;
				while(j<sirpaleita && scount>0){
					if(sirpale[j].aika<=0){
						scount--;
						sirpale[j].z=0;
						sirpale[j].x=x;
						sirpale[j].y=y;
						sirpale[j].xv=xvauhti+random(200)/200.0f-0.5f;
						sirpale[j].yv=yvauhti+random(200)/200.0f-0.5f;
						sirpale[j].zv=random(100)/200.0f-0.25f;
						sirpale[j].maxaika=150;
						sirpale[j].aika=random(120)+50;
						sirpale[j].koko=random(45)/100.0f+0.45f;
						sirpale[j].red=random(50)/100.0f+0.5f;
						sirpale[j].green=random(40)/100.0f+0.0f;
						sirpale[j].blue=random(40)/100.0f+0.0f;
						if(random(10)>7){
							sirpale[j].muoto=10;
							sirpale[j].koko/=2.0f;
						}
						else{
							sirpale[j].muoto=0;
							sirpale[j].aika*=1.5;
						}
					}
					j++;
				}
			}
		}
	}

	//kaasutusmoodi
	if(sets->detail>0){
		if(alus.numero!=22 && alus.numero!=16)
			if(((joinpeli && kaasu) || ((alus.numero==15 && suojapaalla) || alus.numero==18))
			|| (!joinpeli && (pylos || (alus.numero==15 && suojapaalla) || alus.numero==18))) {

				int j=0,scount=sets->detail*1;
				while(j<sirpaleita && scount>0){
					if(sirpale[j].aika<=0){
						scount--;
						if(alus.numero==4){
							if(random(2)<1){
								sirpale[j].x=x+alus.sade*cos((kulma-145)/180*PI)+random(300)/1000.0f-0.15;
								sirpale[j].y=y+alus.sade*sin((kulma-145)/180*PI)+random(300)/1000.0f-0.15;
							}else{
								sirpale[j].x=x+alus.sade*cos((kulma-215)/180*PI)+random(300)/1000.0f-0.15;
								sirpale[j].y=y+alus.sade*sin((kulma-215)/180*PI)+random(300)/1000.0f-0.15;
							}
						}else if(alus.numero==23){
							if(random(2)<1){
								sirpale[j].x=x+alus.sade*cos((kulma-155)/180*PI)+random(300)/1000.0f-0.15;
								sirpale[j].y=y+alus.sade*sin((kulma-155)/180*PI)+random(300)/1000.0f-0.15;
							}else{
								sirpale[j].x=x+alus.sade*cos((kulma-205)/180*PI)+random(300)/1000.0f-0.15;
								sirpale[j].y=y+alus.sade*sin((kulma-205)/180*PI)+random(300)/1000.0f-0.15;
							}
						}else{
							sirpale[j].x=x+alus.sade*cos((kulma-180)/180*PI)+random(300)/1000.0f-0.15;
							sirpale[j].y=y+alus.sade*sin((kulma-180)/180*PI)+random(300)/1000.0f-0.15;
						}
						sirpale[j].z=0;
						sirpale[j].xv=xvauhti+cos((kulma-180)/180*PI)*0.35f+random(200)/1000.0f-0.1;
						sirpale[j].yv=yvauhti+sin((kulma-180)/180*PI)*0.35f+random(200)/1000.0f-0.1;
						sirpale[j].zv=random(200)/1000.0f-0.1;
						sirpale[j].maxaika=100;
						sirpale[j].aika=random(50)+50;
						sirpale[j].koko=random(80)/100.0f+0.15f+alus.sade/5.0f;
						sirpale[j].muoto=0;
						sirpale[j].red=random(60)/100.0f+0.5f;
						sirpale[j].green=random(50)/100.0f;
						sirpale[j].blue=random(50)/100.0f;
						if(random(10)>9){
							sirpale[j].muoto=10;
							sirpale[j].koko/=4.0f;
						}
						else sirpale[j].muoto=0;
					}
					j++;
				}
		}
	}
	if(((joinpeli && kaasu) || ((alus.numero==15 && suojapaalla) || alus.numero==18))
		|| (!joinpeli && (pylos || (alus.numero==15 && suojapaalla) || alus.numero==18))) {
//		if(pylos || (alus.numero==15 && suojapaalla) || alus.numero==18){
			if(alus.numero==6 || alus.numero==18 || alus.numero==22){
				xvauhti=alus.nopeus*cos(kulma/180*PI)*limakerroin;
				yvauhti=alus.nopeus*sin(kulma/180*PI)*limakerroin;
				if(sets->detail<1) kaasujalki[0].alpha=1.0f;
			}else if(alus.easelaji==15 && suojapaalla){
				xvauhti+=alus.nopeus*20*cos(kulma/180*PI)*limakerroin;
				yvauhti+=alus.nopeus*20*sin(kulma/180*PI)*limakerroin;
				xvauhti/=1.0f+kitka*10.9*(fabs(xvauhti)+1.0f);
				yvauhti/=1.0f+kitka*10.9*(fabs(yvauhti)+1.0f);
				if(sets->detail<1) kaasujalki[0].alpha=1.0f;
			}else if(alus.aselaji==11 && suojapaalla){
				xvauhti+=alus.nopeus*2*cos(kulma/180*PI)*limakerroin;
				yvauhti+=alus.nopeus*2*sin(kulma/180*PI)*limakerroin;
				xvauhti/=1.0f+kitka*0.5*(fabs(xvauhti)+1.0f);
				yvauhti/=1.0f+kitka*0.5*(fabs(yvauhti)+1.0f);
				if(sets->detail<1) kaasujalki[0].alpha=1.0f;
			}else {
				xvauhti+=alus.nopeus*cos(kulma/180*PI)*limakerroin;
				yvauhti+=alus.nopeus*sin(kulma/180*PI)*limakerroin;
				xvauhti/=1.0f+kitka*0.9*(fabs(xvauhti)+1.0f);
				yvauhti/=1.0f+kitka*0.9*(fabs(yvauhti)+1.0f);
				if(sets->detail<1) kaasujalki[0].alpha=1.0f;
				if(alus.numero==4 || alus.numero==23) if(sets->detail<1) kaasujalki2[0].alpha=1.0f;
			}
//		}
	}
	int i;
	if(!joinpeli) if(!pylos && (alus.aselaji==6 || alus.numero==22)){ 
		xvauhti=0;yvauhti=0;
	}
//	if(alus.numero==22 && 
	//if(fabs(xvauhti)>0 || fabs(yvauhti)>0){
		for(i=palikkamaara-1;i>0;i--){
			xhanta[i]=xhanta[i-1];
			yhanta[i]=yhanta[i-1];
			kulmahanta[i]=kulmahanta[i-1];
		}
//	}
	if(alus.numero==22){
		xhanta[0]=x;
		yhanta[0]=y;
		kulmahanta[0]=kulma;
	}
	if(palas){
	}
	if((!joinpeli && pleft) || (joinpeli && joinleft)){
		if(alus.numero==15 && suojapaalla){
			if(!joinpeli){
				kulma+=alus.kaantonopeus*1.5*limakerroin;
				asekulma+=alus.kaantonopeus*1.5*limakerroin;
			}
		}
		else if(alus.aselaji==11 && suojapaalla){
			if(!joinpeli){
				kulma+=alus.kaantonopeus/5.0*limakerroin;
				asekulma+=alus.kaantonopeus/5.0*limakerroin;
			}
		}else{
			if(!joinpeli){
				kulma+=alus.kaantonopeus*limakerroin;
				asekulma+=alus.kaantonopeus*limakerroin;
			}
		}
		int k;
		if(alus.aselaji==14) for (k=0;k<alus.maxpadit;k++) if(pati[k].aika>0){
			pati[k].kulma+=alus.kaantonopeus*2;
		}
	}
	if((!joinpeli && pright) || (joinpeli && joinright)){
		if(alus.numero==15 && suojapaalla){
			if(!joinpeli){
				kulma-=alus.kaantonopeus*1.5*limakerroin;
				asekulma-=alus.kaantonopeus*1.5*limakerroin;
			}
		}
		else if(alus.aselaji==11 && suojapaalla){
			if(!joinpeli){
				kulma-=alus.kaantonopeus/5.0*limakerroin;
				asekulma-=alus.kaantonopeus/5.0*limakerroin;
			}
		}else{
			if(!joinpeli){
				kulma-=alus.kaantonopeus*limakerroin;
				asekulma-=alus.kaantonopeus*limakerroin;
			}
		}
		int k;
		if(alus.aselaji==14) for (k=0;k<alus.maxpadit;k++) if(pati[k].aika>0){
			pati[k].kulma-=alus.kaantonopeus*2;
		}
	}
	if(!joinpeli) if(kulma>180) kulma-=360;
	if(!joinpeli) if(kulma<-180)kulma+=360;
	if(ampumisviive>0) ampumisviive--;
	if(!joinpeli)if(fuel<alus.fuel){
		if(alus.aselaji==11 && suojapaalla){
			fuel+=alus.tankkaus/8.0;
		}
		else if(!(alus.aselaji==13)) fuel+=alus.tankkaus;
		else if(pati[0].aika<=0) fuel+=alus.tankkaus;
	} 

	if(!joinpeli) if(alus.aselaji!=6 && alus.aselaji!=18 && alus.numero!=22){
		xvauhti/=1.0f+kitka*0.1*(fabs(xvauhti)+1.0f);
		yvauhti/=1.0f+kitka*0.1*(fabs(yvauhti)+1.0f);
	}

	if(!joinpeli) if(alus.aselaji==6 || alus.aselaji==18 || alus.numero==22){
		if(x>maxx) x-=alus.nopeus;
		if(x<minx) x+=alus.nopeus;
		if(y>maxy) y-=alus.nopeus;
		if(y<miny) y+=alus.nopeus;
	}else if(alus.numero==15 && suojapaalla){
		if(x>maxx) xvauhti-=0.2;
		if(x<minx) xvauhti+=0.2;
		if(y>maxy) yvauhti-=0.2;
		if(y<miny) yvauhti+=0.2;
	}else{
		if(x>maxx) xvauhti-=0.02;
		if(x<minx) xvauhti+=0.02;
		if(y>maxy) yvauhti-=0.02;
		if(y<miny) yvauhti+=0.02;
	}
	if (alus.aselaji == 4 || alus.numero==16 || alus.numero==18){
		//velocity = xvauhti,yvauhti
		//direction = cos(kulma),sin(kulma)
		//propelspeed = dot(velocity,direction)
		double suuntax = cos(kulma/180*PI);
		double suuntay = sin(kulma/180*PI);
		double vauhti = suuntax*xvauhti + suuntay*yvauhti;
		propelli+=vauhti*30;
	}
	if(!joinpeli){
		if(!pfire && laseraani) {
			ampumisaani->stop();
			laseraani=false;
		}
	}else{
		if(!joinase && laseraani) {
			ampumisaani->stop();
			laseraani=false;
		}
	}

	if(!joinpeli) for(i=0;i<players;i++){
		if(player[i].kuolema<=0){
			if(numero<i)if(player[i].tormaa(x+xvauhti, y+yvauhti, alus.sade, numero)) i=i;
		}
	}
	if(!joinpeli) if(alus.easelaji==6 && teleportattu){
		x=uusx;
		y=uusy;
		teleportattu=false;
	}

	if(alus.numero==20 && easeviive>0 && ampumispuoli==3){
		if(easeviive==1) ampumispuoli=1;
		if(efektitpaalla) eampumisaani->play();
		i=alus.easenopeus-easeviive;
		if(!joinpeli){
			epati[i].aika=100;
			epati[i].x=cos((double)i/(double)alus.easenopeus*2.0f*PI+kulma/180*PI)*6.0f+x;
			epati[i].y=sin((double)i/(double)alus.easenopeus*2.0f*PI+kulma/180*PI)*6.0f+y;
			epati[i].xv=cos((double)i/(double)alus.easenopeus*2.0f*PI+kulma/180*PI)*alus.easevauhti+xvauhti;
			epati[i].yv=sin((double)i/(double)alus.easenopeus*2.0f*PI+kulma/180*PI)*alus.easevauhti+yvauhti;
			epati[i].energia=alus.epatienergia;
			epati[i].kulma=atan2(epati[i].yv*100,epati[i].xv*100)*180.0f/PI;
//		epati[i].kulma=0;
		}
	}
	if(alus.numero==19 && easeviive>0 && ampumispuoli>1){
		double ekoko;
		if(easeviive<alus.easenopeus/2){
			ekoko=(easeviive)/20;
		}else{
			ekoko=(alus.easenopeus-easeviive)/20;
		}
		double tempx=x+cos(kulma)*(ekoko*2+alus.sade);
		double tempy=y+sin(kulma)*(ekoko*2+alus.sade);
		for(i=0;i<players;i++) if(i!=numero){
			double xero=tempx-player[i].x;
			double yero=tempy-player[i].y;
			if(xero*xero+yero*yero<pow((player[i].alus.sade+0.2)*0.785,2)){
				if(!joinpeli) player[i].energia-=alus.edamage;
				ampumispuoli=1;
				if(efektitpaalla) eosumisaani->play();
			}
			
		}
	}
	if(ampumisjuttu>0){
		ampumisjuttu-=0.3;
		if(ampumisjuttu<0)ampumisjuttu=0;
	}
	if(!joinpeli) x+=xvauhti;
	if(!joinpeli) y+=yvauhti;
}

bool Player::tormaa(Pati pati2,Alus alus2){
	if(alus.easelaji==0 && suojapaalla) return false;
	if((y-pati2.y)*(y-pati2.y)+(x-pati2.x)*(x-pati2.x)<pow((alus.sade+alus2.patisade)*0.785,2)) return true;
	else return false;
}

bool Player::tormaa(double x2, double y2, double sade2, int i){
	if(sqrt((x+xvauhti-x2)*(x+xvauhti-x2)
		+(y+yvauhti-y2)*(y+yvauhti-y2))<(alus.sade+sade2)*0.785){

			double xet=player[i].x-x;
			double yet=player[i].y-y;
			double kulma5=atan2(yet, xet);
			double xtemp=player[i].xvauhti;
			double ytemp=player[i].yvauhti;

			double tormays=sqrt((xtemp-xvauhti)*(xtemp-xvauhti)+(ytemp-yvauhti)*(ytemp-yvauhti));
//			tormays=tormays*player[i].alus.paino*alus.paino;
			tormays=tormays*(1/(1/player[i].alus.paino+1/alus.paino))*2;
//			double xtormays=(xtemp-xvauhti)/tormays;
//			double ytormays=(ytemp-yvauhti)/tormays;
			
			if(player[i].alus.easelaji==14 || player[i].alus.easelaji==15){
				if(alus.easelaji!=0 || !suojapaalla){
					if(player[i].alus.easelaji==14){
						energia-=player[i].alus.edamage*player[i].fuel/player[i].alus.fuel;
						player[i].fuel=0;
					}
					else energia-=player[i].alus.edamage;
				}
			}
			if(alus.easelaji==14 || alus.easelaji==15){
				if(player[i].alus.easelaji!=0 || !player[i].suojapaalla){
					if(alus.easelaji==14){
						player[i].energia-=alus.edamage*fuel/alus.fuel;
						fuel=0;
					}else player[i].energia-=alus.edamage;
				}
			}
//mass1*mass2 pitäis korvata 1/(1/mass1 + 1/mass2)
			if(player[i].alus.aselaji==6 || player[i].alus.aselaji==18 || player[i].alus.numero==22){
				player[i].x-=(x-player[i].x)*0.2f;
				player[i].y-=(y-player[i].y)*0.2f;
			}else{
//				player[i].xvauhti+=cos(kulma5)*tormays/ player[i].alus.paino*alus.paino;
//				player[i].yvauhti+=sin(kulma5)*tormays/ player[i].alus.paino*alus.paino;
				player[i].xvauhti+=cos(kulma5)*tormays/ player[i].alus.paino;
				player[i].yvauhti+=sin(kulma5)*tormays/ player[i].alus.paino;
			}
			if(alus.aselaji==6 || alus.aselaji==18 || alus.numero==22){
				x-=(player[i].x-x)*0.2f;
				y-=(player[i].y-y)*0.2f;
			}else{
				xvauhti-= cos(kulma5)*tormays / alus.paino;
				yvauhti-= sin(kulma5)*tormays / alus.paino;

			}

			return true;
		
	} else return false;
}

 
