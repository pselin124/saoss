#include "main.h"
#include "Star.h"
#include "net.h"
#include "Define.h"
#include "domination.h"
#include "ctf.h"
#include "sieni.h"
#include "aluspiirto.h"

double random(double x){
	return (rand()*x/RAND_MAX);
}
void setSoundVolumes(){
	alus[0].ampumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[0].eampumisaani->setVolume(0.75*sets->soundvolume/100.0f);
	alus[0].osumisaani->setVolume(0.5*sets->soundvolume/100.0f);

	alus[1].eampumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[1].eosumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[1].ampumisaani->setVolume(1.0*sets->soundvolume/100.0f);
	alus[1].osumisaani->setVolume(1.0*sets->soundvolume/100.0f);

	alus[2].ampumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[2].osumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[2].eampumisaani->setVolume(0.5*sets->soundvolume/100.0f);

	alus[3].ampumisaani->setVolume(0.75*sets->soundvolume/100.0f);
	alus[3].eampumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[3].osumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[3].eosumisaani->setVolume(0.75*sets->soundvolume/100.0f);

	alus[4].osumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[4].ampumisaani->setVolume(0.9*sets->soundvolume/100.0f);
	alus[4].eampumisaani->setVolume(0.75*sets->soundvolume/100.0f);
	alus[4].eosumisaani->setVolume(0.75*sets->soundvolume/100.0f);

	alus[5].ampumisaani->setVolume(0.75*sets->soundvolume/100.0f);
	alus[5].osumisaani->setVolume(0.75*sets->soundvolume/100.0f);
	alus[5].eampumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[5].eosumisaani->setVolume(0.5*sets->soundvolume/100.0f);

	alus[6].ampumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[6].eampumisaani->setVolume(0.125*sets->soundvolume/100.0f);

	alus[7].ampumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[7].eampumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[7].osumisaani->setVolume(1.0*sets->soundvolume/100.0f);

	alus[8].ampumisaani->setVolume(0.75*sets->soundvolume/100.0f);
	alus[8].eampumisaani->setVolume(0.75*sets->soundvolume/100.0f);
	alus[8].eosumisaani->setVolume(0.75*sets->soundvolume/100.0f);
	alus[8].osumisaani->setVolume(0.5*sets->soundvolume/100.0f);

	alus[9].ampumisaani->setVolume(0.75*sets->soundvolume/100.0f);
	alus[9].eampumisaani->setVolume(0.75*sets->soundvolume/100.0f);
	alus[9].eosumisaani->setVolume(0.75*sets->soundvolume/100.0f);
	alus[9].osumisaani->setVolume(0.5*sets->soundvolume/100.0f);

	alus[10].ampumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[10].eampumisaani->setVolume(0.8*sets->soundvolume/100.0f);
	alus[10].osumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[10].eosumisaani->setVolume(0.3*sets->soundvolume/100.0f);

	alus[11].ampumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[11].eampumisaani->setVolume(0.3*sets->soundvolume/100.0f);
	alus[11].osumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[11].eosumisaani->setVolume(0.25*sets->soundvolume/100.0f);

	alus[12].ampumisaani->setVolume(0.34*sets->soundvolume/100.0f);
	alus[12].eampumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[12].osumisaani->setVolume(0.34*sets->soundvolume/100.0f);
	alus[12].eosumisaani->setVolume(0.75*sets->soundvolume/100.0f);

	alus[13].ampumisaani->setVolume(0.6*sets->soundvolume/100.0f);
	alus[13].eampumisaani->setVolume(0.6*sets->soundvolume/100.0f);
	alus[13].osumisaani->setVolume(0.8*sets->soundvolume/100.0f);
	alus[13].eosumisaani->setVolume(0);

	alus[14].ampumisaani->setVolume(0.99*sets->soundvolume/100.0f);
	alus[14].eampumisaani->setVolume(0.99*sets->soundvolume/100.0f);
	alus[14].osumisaani->setVolume(0.6*sets->soundvolume/100.0f);
	alus[14].eosumisaani->setVolume(0);

	alus[15].ampumisaani->setVolume(1.0*sets->soundvolume/100.0f);
	alus[15].eampumisaani->setVolume(1.0*sets->soundvolume/100.0f);
	alus[15].osumisaani->setVolume(0);
	alus[15].eosumisaani->setVolume(0);

	alus[16].ampumisaani->setVolume(0.8*sets->soundvolume/100.0f);
	alus[16].eampumisaani->setVolume(0.8*sets->soundvolume/100.0f);
	alus[16].osumisaani->setVolume(0.7*sets->soundvolume/100.0f);
	alus[16].eosumisaani->setVolume(0.5*sets->soundvolume/100.0f);

	alus[17].ampumisaani->setVolume(0.6*sets->soundvolume/100.0f);
	alus[17].eampumisaani->setVolume(0.6*sets->soundvolume/100.0f);
	alus[17].osumisaani->setVolume(1.0*sets->soundvolume/100.0f);
	alus[17].eosumisaani->setVolume(1.0*sets->soundvolume/100.0f);

	alus[18].ampumisaani->setVolume(0.8*sets->soundvolume/100.0f);
	alus[18].eampumisaani->setVolume(0.6*sets->soundvolume/100.0f);
	alus[18].osumisaani->setVolume(0.0*sets->soundvolume/100.0f);
	alus[18].eosumisaani->setVolume(0.0*sets->soundvolume/100.0f);

	alus[19].ampumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[19].eampumisaani->setVolume(0.6*sets->soundvolume/100.0f);
	alus[19].osumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[19].eosumisaani->setVolume(1.0*sets->soundvolume/100.0f);

	alus[20].ampumisaani->setVolume(0.7*sets->soundvolume/100.0f);
	alus[20].eampumisaani->setVolume(0.5*sets->soundvolume/100.0f);
	alus[20].osumisaani->setVolume(0.9*sets->soundvolume/100.0f);
	alus[20].eosumisaani->setVolume(0.5*sets->soundvolume/100.0f);

	alus[21].ampumisaani->setVolume(0.0*sets->soundvolume/100.0f);
	alus[21].eampumisaani->setVolume(0.0*sets->soundvolume/100.0f);
	alus[21].osumisaani->setVolume(0.0*sets->soundvolume/100.0f);
	alus[21].eosumisaani->setVolume(0.0*sets->soundvolume/100.0f);

	alus[22].ampumisaani->setVolume(0.0*sets->soundvolume/100.0f);
	alus[22].eampumisaani->setVolume(0.0*sets->soundvolume/100.0f);
	alus[22].osumisaani->setVolume(0.0*sets->soundvolume/100.0f);
	alus[22].eosumisaani->setVolume(0.0*sets->soundvolume/100.0f);

	alus[23].ampumisaani->setVolume(0.0*sets->soundvolume/100.0f);
	alus[23].eampumisaani->setVolume(0.0*sets->soundvolume/100.0f);
	alus[23].osumisaani->setVolume(0.0*sets->soundvolume/100.0f);
	alus[23].eosumisaani->setVolume(0.0*sets->soundvolume/100.0f);
}

pikkumiessiirto siirrot[10];
void alota(){

	sprintf(alus[0].nimi,"Ville");
	alus[0].numero=0;
	alus[0].paino=200;
	alus[0].nopeus=0.0015;
	alus[0].sade=2.6f;
	alus[0].aselaji=0;
	alus[0].ampumisnopeus=8;
	alus[0].patiaika=80;
	alus[0].maxpadit=40;
	alus[0].patinopeus=0.3f;
	alus[0].patisade=0.4;
	alus[0].damage=1;
	alus[0].energia=50;
	alus[0].patipotkasu=0.01;
	alus[0].jalkikoko=0.13;
	alus[0].fuel=100;
	alus[0].tankkaus=0.17;
	alus[0].patifuel=5;
	alus[0].easelaji=0;
	alus[0].kaantonopeus=1.0f;
	alus[0].easefuel=27.0f;
	alus[0].easekesto=125;
	alus[0].easenopeus=50;
	alus[0].maxepadit=0;
	alus[0].edamage=0;
	alus[0].epatisade=0;
	alus[0].epatipotkasu=0;
	alus[0].patihajottaa=true;
	alus[0].epatihajottaa=false;
	alus[0].patiVoiHajota=true;
	alus[0].epatiVoiHajota=false;
	alus[0].patienergia=3; 
	alus[0].epatienergia=0;
	alus[0].pistemaara=19;

	if(!dserver){
	alus[0].eosumisaani=NULL;
	alus[0].ampumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[0].eampumisaani=new Sound("sounds\\a_0_eampumis.wav");
	alus[0].osumisaani=new Sound(alus[0].ampumisaani);
	}

	
	sprintf(alus[1].nimi,"Ville");
	alus[1].numero=1;
	alus[1].paino=100;
	alus[1].nopeus=0.003;
	alus[1].sade=2.4f;
	alus[1].aselaji=1;
	alus[1].ampumisnopeus=70;
	alus[1].patiaika=70;
	alus[1].maxpadit=5;
	alus[1].patinopeus=0.4;
	alus[1].patisade=1.8;
	alus[1].damage=20;
	alus[1].energia=15;
	alus[1].patipotkasu=1;
	alus[1].epatipotkasu=0.05;
	alus[1].jalkikoko=0.12;
	alus[1].fuel=60;
	alus[1].tankkaus=0.04;
	alus[1].patifuel=30;
	alus[1].easelaji=1;
	alus[1].kaantonopeus=2;
	alus[1].easefuel=43;
	alus[1].easekesto=50;
	alus[1].easevauhti=0.5;
	alus[1].easenopeus=100;
	alus[1].maxepadit=100;
	alus[1].edamage=1;
	alus[1].epatisade=0.3f;
	alus[1].patihajottaa=true;
	alus[1].epatihajottaa=true;
	alus[1].patiVoiHajota=true;
	alus[1].epatiVoiHajota=true;
	alus[1].patienergia=100; 
	alus[1].epatienergia=1;
	alus[1].pistemaara=16;

	if(!dserver){
		alus[1].eampumisaani=new Sound(alus[0].ampumisaani);
		alus[1].eosumisaani=new Sound(alus[0].osumisaani);
		alus[1].ampumisaani=new Sound("sounds\\a_1_ampumis.wav");
		alus[1].osumisaani=new Sound("sounds\\a_1_osumis.wav");
	}

	//printf("Möö\r\n");
	
	sprintf(alus[2].nimi,"Ville");
	alus[2].numero=2;
	alus[2].paino=100;
	alus[2].nopeus=0.0048;
	alus[2].sade=1.2f;
	alus[2].aselaji=2;
	alus[2].ampumisnopeus=3;
	alus[2].patiaika=30;
	alus[2].maxpadit=40;
	alus[2].patinopeus=1.0;
	alus[2].patisade=0.2;
	alus[2].damage=0.7;
	alus[2].energia=13;
	alus[2].patipotkasu=0.005;
	alus[2].jalkikoko=0.05;
	alus[2].fuel=30;
	alus[2].tankkaus=0.2;
	alus[2].patifuel=2;
	alus[2].easelaji=2;
	alus[2].kaantonopeus=3.0f;
	alus[2].easefuel=1.0f;
	alus[2].easekesto=0;
	alus[2].easenopeus=0;
	alus[2].maxepadit=0;
	alus[2].edamage=100;
	alus[2].epatisade=0;
	alus[2].epatipotkasu=0;
	alus[2].patihajottaa=true;
	alus[2].epatihajottaa=false;
	alus[2].patiVoiHajota=true;
	alus[2].epatiVoiHajota=false;
	alus[2].patienergia=1; 
	alus[2].epatienergia=0;
	alus[2].pistemaara=9;

	if(!dserver){
	alus[2].ampumisaani=new Sound(alus[0].ampumisaani);
	alus[2].osumisaani=new Sound(alus[0].ampumisaani);
	alus[2].eampumisaani=new Sound("sounds\\a_2_eampumis.wav");
	alus[2].eampumisaani->setVolume(128);
	alus[2].eosumisaani=NULL;
	}
	//if(alus[2].eampumisaani!=NULL) Mix_VolumeChunk(alus[2].eampumisaani, 64);	

	//printf("Möö\r\n");

	sprintf(alus[3].nimi,"Ville");
	alus[3].numero=3;
	alus[3].paino=150;
	alus[3].nopeus=0.003;
	alus[3].sade=2.8f;
	alus[3].aselaji=3;
	alus[3].ampumisnopeus=16;
	alus[3].patiaika=40;
	alus[3].maxpadit=40;
	alus[3].patinopeus=0.8;
	alus[3].patisade=0.7;
	alus[3].damage=3;
	alus[3].energia=40;
	alus[3].patipotkasu=0.1;
	alus[3].epatipotkasu=0.05;
	alus[3].jalkikoko=0.15;
	alus[3].fuel=40;
	alus[3].tankkaus=0.1;
	alus[3].patifuel=8;
	alus[3].easelaji=3;
	alus[3].kaantonopeus=2.3;
	alus[3].easefuel=15;
	alus[3].easekesto=150;
	alus[3].easenopeus=50;
	alus[3].easevauhti=0.25;
	alus[3].maxepadit=40;
	alus[3].edamage=5;
	alus[3].epatisade=0.6f;
	alus[3].patihajottaa=true;
	alus[3].epatihajottaa=true;
	alus[3].patiVoiHajota=true;
	alus[3].epatiVoiHajota=true;
	alus[3].patienergia=3; 
	alus[3].epatienergia=3;
	alus[3].pistemaara=18;

	if(!dserver){
	alus[3].ampumisaani=new Sound("sounds\\a_3_ampumis.wav");
	alus[3].osumisaani=new Sound("sounds\\a_1_osumis.wav");
	alus[3].eampumisaani=new Sound("sounds\\a_1_ampumis.wav");
	alus[3].eosumisaani=new Sound("sounds\\a_3_eosumis.wav");
	}
/*	if(alus[3].ampumisaani!=NULL) Mix_VolumeChunk(alus[3].ampumisaani, 96);	
	if(alus[3].eampumisaani!=NULL) Mix_VolumeChunk(alus[3].eampumisaani, 64);	
	if(alus[3].eosumisaani!=NULL) Mix_VolumeChunk(alus[3].eosumisaani, 96);	
	if(alus[3].osumisaani!=NULL) Mix_VolumeChunk(alus[3].osumisaani, 64);	
*/


	sprintf(alus[4].nimi,"Ville");
	alus[4].numero=4;
	alus[4].paino=500;
	alus[4].nopeus=0.0015;
	alus[4].sade=4.4f;
	alus[4].aselaji=4;
	alus[4].ampumisnopeus=26;
	alus[4].patiaika=1000;
	alus[4].maxpadit=80;
	alus[4].patinopeus=0.8;
	alus[4].patisade=0.15;
	alus[4].damage=17;
	alus[4].energia=50;
	alus[4].patipotkasu=0.2;
	alus[4].epatipotkasu=0.1;
	alus[4].jalkikoko=0.22;
	alus[4].fuel=70;
	alus[4].tankkaus=0.13;
	alus[4].patifuel=30;
	alus[4].easelaji=4;
	alus[4].kaantonopeus=0.8;
	alus[4].easefuel=60;
	alus[4].easekesto=20000;
	alus[4].easenopeus=30;
	alus[4].easevauhti=0.0055;
	alus[4].maxepadit=5;
	alus[4].edamage=2.5;
	alus[4].epatisade=1.4f;
	alus[4].patihajottaa=true;
	alus[4].epatihajottaa=false;
	alus[4].patiVoiHajota=true;
	alus[4].epatiVoiHajota=true;
	alus[4].patienergia=15; 
	alus[4].epatienergia=25;
	alus[4].pistemaara=30;

	if(!dserver){
	alus[4].eosumisaani=new Sound("sounds\\a_4_eosumis.wav");
	alus[4].ampumisaani=new Sound("sounds\\a_4_ampumis.wav");
	alus[4].eampumisaani=new Sound("sounds\\a_4_eampumis.wav");
	alus[4].osumisaani=new Sound(alus[0].osumisaani);
	}
/*f(alus[4].eosumisaani!=NULL) Mix_VolumeChunk(alus[4].eosumisaani, 96);	
	if(alus[4].ampumisaani!=NULL) Mix_VolumeChunk(alus[4].ampumisaani, 110);	
	if(alus[4].eampumisaani!=NULL) Mix_VolumeChunk(alus[4].eampumisaani, 96);	
*/


	sprintf(alus[5].nimi,"Ville");
	alus[5].numero=5;
	alus[5].paino=500;
	alus[5].nopeus=0.0017;
	alus[5].sade=4.4f;
	alus[5].aselaji=5;
	alus[5].ampumisnopeus=60;
	alus[5].patiaika=70;
	alus[5].maxpadit=2;
	alus[5].patinopeus=0.6;
	alus[5].patisade=4;
	alus[5].damage=19;
	alus[5].energia=60;
	alus[5].patipotkasu=0.4;
	alus[5].epatipotkasu=0;
	alus[5].jalkikoko=0.22;
	alus[5].fuel=70;
	alus[5].tankkaus=0.08;
	alus[5].patifuel=25;
	alus[5].easelaji=5;
	alus[5].kaantonopeus=0.7;
	alus[5].easefuel=6;
	alus[5].easekesto=5000;
	alus[5].easenopeus=30;
	alus[5].easevauhti=0.24;
	alus[5].maxepadit=60;
	alus[5].edamage=0.12;
	alus[5].epatisade=0.45f;
	alus[5].patihajottaa=true;
	alus[5].epatihajottaa=false;
	alus[5].patiVoiHajota=true;
	alus[5].epatiVoiHajota=true;
	alus[5].patienergia=50; 
	alus[5].epatienergia=2.5;
	alus[5].pistemaara=33;

	if(!dserver){
	alus[5].ampumisaani=new Sound("sounds\\a_5_ampumis.wav");
	alus[5].osumisaani=new Sound("sounds\\a_5_osumis.wav");
	alus[5].eampumisaani=new Sound(alus[0].ampumisaani);
	alus[5].eosumisaani=new Sound(alus[5].eampumisaani);
	}	
/*	if(alus[5].ampumisaani!=NULL) Mix_VolumeChunk(alus[5].ampumisaani, 96);	
	if(alus[5].osumisaani!=NULL) Mix_VolumeChunk(alus[5].osumisaani, 96);	
//	if(alus[5].eampumisaani!=NULL) Mix_VolumeChunk(alus[5].eampumisaani, 64);	
*/

//	printf("Möö\r\n");



	sprintf(alus[6].nimi,"Ville");
	alus[6].numero=6;
	alus[6].paino=100;
	alus[6].nopeus=0.31;
	alus[6].sade=2.0f;
	alus[6].aselaji=6;
	alus[6].ampumisnopeus=10;
	alus[6].patiaika=2;
	alus[6].maxpadit=1;
	alus[6].patinopeus=0;
	alus[6].patisade=14.0f;
	alus[6].damage=2.0f;
	alus[6].energia=21;
	alus[6].patipotkasu=0;
	alus[6].epatipotkasu=0;
	alus[6].jalkikoko=0.12;
	alus[6].fuel=30;
	alus[6].tankkaus=0.08;
	alus[6].patifuel=6;
	alus[6].easelaji=6;
	alus[6].kaantonopeus=3.5f;
	alus[6].easefuel=6;
	alus[6].easekesto=0;
	alus[6].easenopeus=100;
	alus[6].easevauhti=0;
	alus[6].maxepadit=0;
	alus[6].edamage=0;
	alus[6].epatisade=0;
	alus[6].patihajottaa=false;
	alus[6].epatihajottaa=false;
	alus[6].patiVoiHajota=false;
	alus[6].epatiVoiHajota=false;
	alus[6].patienergia=0; 
	alus[6].epatienergia=0;
	alus[6].pistemaara=18;

	if(!dserver){
	alus[6].ampumisaani=new Sound(alus[0].ampumisaani);
	alus[6].eampumisaani=new Sound("sounds\\a_6_eampumis.wav");
	alus[6].osumisaani=NULL;
	alus[6].eosumisaani=NULL;
	
	alus[6].eampumisaani->setVolume(32);
	}
//	if(alus[6].eampumisaani!=NULL) Mix_VolumeChunk(alus[6].eampumisaani, 16);	



	sprintf(alus[7].nimi,"Ville");
	alus[7].numero=7;
	alus[7].paino=180;
	alus[7].nopeus=0.0015;
	alus[7].sade=2.8f;
	alus[7].aselaji=7;
	alus[7].ampumisnopeus=50;
	alus[7].patiaika=500;
	alus[7].maxpadit=5;
	alus[7].patinopeus=0.008;
	alus[7].patisade=1.0f;
	alus[7].damage=5;
	alus[7].energia=35;
	alus[7].patipotkasu=0.7;
	alus[7].epatipotkasu=0;
	alus[7].jalkikoko=0.17;
	alus[7].fuel=30;
	alus[7].tankkaus=0.08;
	alus[7].patifuel=20;
	alus[7].easelaji=7;
	alus[7].kaantonopeus=1.5f;
	alus[7].easefuel=5;
	alus[7].easekesto=2;
	alus[7].easenopeus=35;
	alus[7].easevauhti=0;
	alus[7].maxepadit=0;
	alus[7].edamage=2.1f;
	alus[7].epatisade=4.5f;
	alus[7].patihajottaa=true;
	alus[7].epatihajottaa=false;
	alus[7].patiVoiHajota=true;
	alus[7].epatiVoiHajota=false;
	alus[7].patienergia=4; 
	alus[7].epatienergia=0;
	alus[7].pistemaara=21;

	if(!dserver){
	alus[7].ampumisaani=new Sound("sounds\\a_7_ampumis.wav");
	alus[7].eampumisaani=new Sound(alus[0].ampumisaani);
	alus[7].osumisaani=new Sound(alus[1].osumisaani);
	alus[7].eosumisaani=NULL;
	
	alus[7].ampumisaani->setVolume(128);
	}
	//if(alus[7].ampumisaani!=NULL) Mix_VolumeChunk(alus[7].ampumisaani, 64);	
//	if(alus[7].eampumisaani!=NULL) Mix_VolumeChunk(alus[7].eampumisaani, 16);	



	sprintf(alus[8].nimi,"Ville");
	alus[8].numero=8;
	alus[8].paino=150;
	alus[8].nopeus=0.0030;
	alus[8].sade=2.4f;
	alus[8].aselaji=8;
	alus[8].ampumisnopeus=2;
	alus[8].patiaika=45;
	alus[8].maxpadit=200;
	alus[8].patinopeus=0.5;
	alus[8].patisade=0.2f;
	alus[8].damage=0.3f;
	alus[8].energia=30;
	alus[8].patipotkasu=0.002;
	alus[8].epatipotkasu=0.5;
	alus[8].jalkikoko=0.13;
	alus[8].fuel=40;
	alus[8].tankkaus=0.08;
	alus[8].patifuel=0.6;
	alus[8].easelaji=8;
	alus[8].kaantonopeus=2.4f;
	alus[8].easefuel=15;
	alus[8].easekesto=1000;
	alus[8].easenopeus=35;
	alus[8].easevauhti=0.4;
	alus[8].maxepadit=50;
	alus[8].edamage=4.0f;
	alus[8].epatisade=1.0f;
	alus[8].patihajottaa=true;
	alus[8].epatihajottaa=true;
	alus[8].patiVoiHajota=true;
	alus[8].epatiVoiHajota=false;
	alus[8].patienergia=0.4; 
	alus[8].epatienergia=300;
	alus[8].pistemaara=16;

	if(!dserver){
	alus[8].ampumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[8].eampumisaani=new Sound(alus[5].ampumisaani);
	alus[8].eosumisaani=new Sound(alus[5].osumisaani);
	alus[8].osumisaani=new Sound(alus[0].osumisaani);

	alus[8].ampumisaani->setVolume(192);
	}
//	if(alus[8].ampumisaani!=NULL) Mix_VolumeChunk(alus[8].ampumisaani, 16);	

//	printf("Möö\r\n");


	sprintf(alus[9].nimi,"Ville");
	alus[9].numero=9;
	alus[9].paino=200;
	alus[9].nopeus=0.0022;
	alus[9].sade=2.8f;
	alus[9].aselaji=9;
	alus[9].ampumisnopeus=1;
	alus[9].patiaika=2;
	alus[9].maxpadit=0;
	alus[9].patinopeus=0;
	alus[9].patisade=17;
	alus[9].damage=0.25f;
	alus[9].energia=35;
	alus[9].patipotkasu=0;
	alus[9].epatipotkasu=0;
	alus[9].jalkikoko=0.14;
	alus[9].fuel=40;
	alus[9].tankkaus=0.03;
	alus[9].patifuel=0.3;
	alus[9].easelaji=9;
	alus[9].kaantonopeus=1.1f;
	alus[9].easefuel=30;
	alus[9].easekesto=3000;
	alus[9].easenopeus=35;
	alus[9].easevauhti=0.3;
	alus[9].maxepadit=20;
	alus[9].edamage=1.5f;
	alus[9].epatisade=1.0f;
	alus[9].patihajottaa=false;
	alus[9].epatihajottaa=false;
	alus[9].patiVoiHajota=false;
	alus[9].epatiVoiHajota=false;
	alus[9].patienergia=0; 
	alus[9].epatienergia=300;
	alus[9].pistemaara=17;

	if(!dserver){
	alus[9].ampumisaani=new Sound("sounds\\a_9_ampumis.wav", true);
	alus[9].eampumisaani=new Sound(alus[5].ampumisaani);
	alus[9].eosumisaani=new Sound(alus[5].osumisaani);
	alus[9].osumisaani=new Sound(alus[0].osumisaani);
	}


	sprintf(alus[10].nimi,"Ville");
	alus[10].numero=10;
	alus[10].paino=180;
	alus[10].nopeus=0.002;
	alus[10].sade=2.4f;
	alus[10].aselaji=10;
	alus[10].ampumisnopeus=40;
	alus[10].patiaika=100;
	alus[10].maxpadit=10;
	alus[10].patinopeus=0.45f;
	alus[10].patisade=0.4;
	alus[10].damage=1.5;
	alus[10].energia=25;
	alus[10].patipotkasu=0.02;
	alus[10].jalkikoko=0.12;
	alus[10].fuel=40;
	alus[10].tankkaus=0.1;
	alus[10].patifuel=6;
	alus[10].easelaji=10;
	alus[10].kaantonopeus=2.5f;
	alus[10].easefuel=12.0f;
	alus[10].easekesto=3000;
	alus[10].easenopeus=350;
	alus[10].maxepadit=0;
	alus[10].edamage=5;
	alus[10].epatisade=14;
	alus[10].epatipotkasu=0;
	alus[10].patihajottaa=true;
	alus[10].epatihajottaa=false;
	alus[10].patiVoiHajota=true;
	alus[10].epatiVoiHajota=false;
	alus[10].patienergia=7; 
	alus[10].epatienergia=5;
	alus[10].pistemaara=15;

	if(!dserver){
	alus[10].ampumisaani=new Sound("sounds\\a_10_ampumis.wav");
	alus[10].eampumisaani=new Sound("sounds\\a_10_eampumis.wav");
	alus[10].osumisaani=new Sound(alus[10].ampumisaani);
	alus[10].eosumisaani=new Sound("sounds\\a_0_ampumis.wav");

	alus[10].ampumisaani->setVolume(128);
	alus[10].eampumisaani->setVolume(80);
	alus[10].eosumisaani->setVolume(80);
	}
/*	if(alus[10].ampumisaani!=NULL) Mix_VolumeChunk(alus[10].ampumisaani, 64);
	if(alus[10].eampumisaani!=NULL)Mix_VolumeChunk(alus[10].eampumisaani, 40);
	if(alus[10].eosumisaani!=NULL)Mix_VolumeChunk(alus[10].eosumisaani, 40);
*/

	sprintf(alus[11].nimi,"Ville");
	alus[11].numero=11;
	alus[11].paino=200;
	alus[11].nopeus=0.0012;
	alus[11].sade=3.0f;
	alus[11].aselaji=11;
	alus[11].ampumisnopeus=50;
	alus[11].patiaika=110;
	alus[11].maxpadit=20;
	alus[11].patinopeus=0.65f;
	alus[11].patisade=0.7;
	alus[11].damage=3.2;
	alus[11].energia=30;
	alus[11].patipotkasu=0.01;
	alus[11].jalkikoko=0.13;
	alus[11].fuel=40;
	alus[11].tankkaus=0.15;
	alus[11].patifuel=10;
	alus[11].easelaji=11;
	alus[11].kaantonopeus=2.5f;
	alus[11].easefuel=8.0f;
	alus[11].easekesto=2;
	alus[11].easenopeus=200;
	alus[11].maxepadit=0;
	alus[11].edamage=12;
	alus[11].epatisade=11;
	alus[11].epatipotkasu=0;
	alus[11].patihajottaa=true;
	alus[11].epatihajottaa=false;
	alus[11].patiVoiHajota=true;
	alus[11].epatiVoiHajota=false;
	alus[11].patienergia=6; 
	alus[11].epatienergia=0;
	alus[11].pistemaara=20;

	if(!dserver){
	alus[11].ampumisaani=new Sound("sounds\\a_11_ampumis.wav");
	alus[11].eampumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[11].osumisaani=new Sound(alus[10].ampumisaani);
	alus[11].eosumisaani=new Sound("sounds\\a_11_eosumis.wav");
	
	alus[11].ampumisaani->setVolume(128);
	alus[11].eampumisaani->setVolume(80);
	alus[11].eosumisaani->setVolume(64);
	}
/*	if(alus[11].ampumisaani!=NULL) Mix_VolumeChunk(alus[11].ampumisaani, 64);
	if(alus[11].eampumisaani!=NULL)Mix_VolumeChunk(alus[11].eampumisaani, 40);
	if(alus[11].eosumisaani!=NULL)Mix_VolumeChunk(alus[11].eosumisaani, 32);
*/

	sprintf(alus[12].nimi,"Ville");
	alus[12].numero=12;
	alus[12].paino=150;
	alus[12].nopeus=0.0033;
	alus[12].sade=2.4f;
	alus[12].aselaji=12;
	alus[12].ampumisnopeus=3;
	alus[12].patiaika=45;
	alus[12].maxpadit=100;
	alus[12].patinopeus=0.3f;
	alus[12].patisade=0.4;
	alus[12].damage=0.7;
	alus[12].energia=20;
	alus[12].patipotkasu=0;
	alus[12].jalkikoko=0.10;
	alus[12].fuel=30;
	alus[12].tankkaus=0;
	alus[12].patifuel=0.7;
	alus[12].easelaji=12;
	alus[12].kaantonopeus=3.2f;
	alus[12].easefuel=0;
	alus[12].easekesto=0;
	alus[12].easenopeus=200;
	alus[12].maxepadit=0;
	alus[12].edamage=0;
	alus[12].epatisade=0;
	alus[12].epatipotkasu=0;
	alus[12].patihajottaa=true;
	alus[12].epatihajottaa=false;
	alus[12].patiVoiHajota=true;
	alus[12].epatiVoiHajota=false;
	alus[12].patienergia=2; 
	alus[12].epatienergia=0;
	alus[12].pistemaara=16;

	if(!dserver){
	alus[12].ampumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[12].eampumisaani=new Sound("sounds\\a_0_eampumis.wav");
	alus[12].osumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[12].eosumisaani=new Sound("sounds\\a_6_eampumis.wav");

	alus[12].ampumisaani->setVolume(128);
	alus[12].eampumisaani->setVolume(128);
	alus[12].osumisaani->setVolume(128);
	alus[12].eosumisaani->setVolume(192);
	}
/*	if(alus[12].ampumisaani!=NULL) Mix_VolumeChunk(alus[12].ampumisaani, 64);
	if(alus[12].osumisaani!=NULL)Mix_VolumeChunk(alus[12].osumisaani, 64);
	if(alus[12].eampumisaani!=NULL)Mix_VolumeChunk(alus[12].eampumisaani, 64);
	if(alus[12].eosumisaani!=NULL)Mix_VolumeChunk(alus[12].eosumisaani, 96);
*/

	sprintf(alus[13].nimi,"Ville");
	alus[13].numero=13;
	alus[13].paino=250;
	alus[13].nopeus=0.0018;
	alus[13].sade=3.6f;
	alus[13].aselaji=13;
	alus[13].ampumisnopeus=100;
	alus[13].patiaika=100;
	alus[13].maxpadit=10;
	alus[13].patinopeus=0.5f;
	alus[13].patisade=3.0;
	alus[13].damage=40;
	alus[13].energia=30;
	alus[13].patipotkasu=0;
	alus[13].jalkikoko=0.18;
	alus[13].fuel=30;
	alus[13].tankkaus=0.11;
	alus[13].patifuel=0;
	alus[13].easelaji=13;
	alus[13].kaantonopeus=1.2f;
	alus[13].easefuel=3;
	alus[13].easekesto=325;
	alus[13].easenopeus=20;
	alus[13].easevauhti=0.21;
	alus[13].maxepadit=100;
	alus[13].edamage=2;
	alus[13].epatisade=1.0;
	alus[13].epatipotkasu=0;
	alus[13].patihajottaa=true;
	alus[13].epatihajottaa=true;
	alus[13].patiVoiHajota=false;
	alus[13].epatiVoiHajota=true;
	alus[13].patienergia=100; 
	alus[13].epatienergia=2;
	alus[13].pistemaara=26;

	if(!dserver){
	alus[13].ampumisaani=new Sound("sounds\\a_13_ampumis.wav");
	alus[13].eampumisaani=new Sound("sounds\\a_13_eampumis.wav");
	alus[13].osumisaani=new Sound("sounds\\a_13_osumis.wav");
	alus[13].eosumisaani=new Sound("sounds\\a_0_ampumis.wav");
	}

	sprintf(alus[14].nimi,"ville");
	alus[14].numero=14;
	alus[14].paino=250;
	alus[14].nopeus=0.0018;
	alus[14].sade=3.0f;
	alus[14].aselaji=14;
	alus[14].ampumisnopeus=150;
	alus[14].patiaika=400;
	alus[14].maxpadit=10;
	alus[14].patinopeus=0.007f;
	alus[14].patisade=2.0;
	alus[14].damage=5;
	alus[14].energia=33;
	alus[14].patipotkasu=0.2;
	alus[14].jalkikoko=0.16;
	alus[14].fuel=25;
	alus[14].tankkaus=0.05;
	alus[14].patifuel=10;
	alus[14].easelaji=14;
	alus[14].kaantonopeus=1.2f;
	alus[14].easefuel=5;
	alus[14].easekesto=0;
	alus[14].easenopeus=50;
	alus[14].easevauhti=0.5;
	alus[14].maxepadit=0;
	alus[14].edamage=10;
	alus[14].epatisade=0;
	alus[14].epatipotkasu=0;
	alus[14].patihajottaa=true;
	alus[14].epatihajottaa=false;
	alus[14].patiVoiHajota=true;
	alus[14].epatiVoiHajota=false;
	alus[14].patienergia=10; 
	alus[14].epatienergia=0;
	alus[14].pistemaara=19;

	
	if(!dserver){
	alus[14].ampumisaani=new Sound("sounds\\a_14_ampumis.wav");
	alus[14].eampumisaani=new Sound("sounds\\a_14_eampumis.wav");
	alus[14].osumisaani=new Sound("sounds\\a_14_osumis.wav");
	alus[14].eosumisaani=new Sound("sounds\\a_0_ampumis.wav");
	}
	
	sprintf(alus[15].nimi,"Ville");
	alus[15].numero=15;
	alus[15].paino=150;
	alus[15].nopeus=0.002;
	alus[15].sade=2.4f;
	alus[15].aselaji=15;
	alus[15].ampumisnopeus=4;
	alus[15].patiaika=4000;
	alus[15].maxpadit=200;
	alus[15].patinopeus=0.05f;
	alus[15].patisade=1.0;
	alus[15].damage=1;
	alus[15].energia=28;
	alus[15].patipotkasu=0;
	alus[15].jalkikoko=0.13;
	alus[15].fuel=25;
	alus[15].tankkaus=0.05;
	alus[15].patifuel=2.5;
	alus[15].easelaji=15;
	alus[15].kaantonopeus=1.8f;
	alus[15].easefuel=0.07;
	alus[15].easekesto=0;
	alus[15].easenopeus=50;
	alus[15].easevauhti=0.5;
	alus[15].maxepadit=0;
	alus[15].edamage=4;
	alus[15].epatisade=0;
	alus[15].epatipotkasu=0;
	alus[15].patihajottaa=false;
	alus[15].epatihajottaa=false;
	alus[15].patiVoiHajota=false;
	alus[15].epatiVoiHajota=false;
	alus[15].patienergia=10; 
	alus[15].epatienergia=0;
	alus[15].pistemaara=17;

	
	if(!dserver){
	alus[15].ampumisaani=new Sound("sounds\\a_15_ampumis.wav");
	alus[15].eampumisaani=new Sound("sounds\\a_15_eampumis.wav");
	alus[15].osumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[15].eosumisaani=new Sound("sounds\\a_0_ampumis.wav");
	}

	

	sprintf(alus[16].nimi,"Ville");
	alus[16].numero=16;
	alus[16].paino=550;
	alus[16].nopeus=0.0018;
	alus[16].sade=5.0f;
	alus[16].aselaji=16;
	alus[16].ampumisnopeus=10;
	alus[16].patiaika=120;
	alus[16].maxpadit=50;
	alus[16].patinopeus=0.5f;
	alus[16].patisade=2.4;
	alus[16].damage=3.5;
	alus[16].energia=65;
	alus[16].patipotkasu=0.1;
	alus[16].jalkikoko=0.24;
	alus[16].fuel=50;
	alus[16].tankkaus=0.10;
	alus[16].patifuel=4.5;
	alus[16].easelaji=16;
	alus[16].kaantonopeus=1.0f;
	alus[16].easefuel=20;
	alus[16].easekesto=70;
	alus[16].easenopeus=50;
	alus[16].easevauhti=0.5;
	alus[16].maxepadit=1000;
	alus[16].edamage=1.5;
	alus[16].epatisade=0.7;
	alus[16].epatipotkasu=0.1;
	alus[16].patihajottaa=true;
	alus[16].epatihajottaa=true;
	alus[16].patiVoiHajota=true;
	alus[16].epatiVoiHajota=true;
	alus[16].patienergia=6; 
	alus[16].epatienergia=12;
	alus[16].pistemaara=47;

	
	if(!dserver){
	alus[16].ampumisaani=new Sound("sounds\\a_16_ampumis.wav");
	alus[16].eampumisaani=new Sound("sounds\\a_16_eampumis.wav");
	alus[16].osumisaani=new Sound("sounds\\a_16_osumis.wav");
	alus[16].eosumisaani=new Sound("sounds\\a_0_ampumis.wav");
	}
	
	
	
	sprintf(alus[17].nimi,"Ville");
	alus[17].numero=17;
	alus[17].paino=75;
	alus[17].nopeus=0.0019;
	alus[17].sade=2.0f;
	alus[17].aselaji=17;
	alus[17].ampumisnopeus=40;
	alus[17].patiaika=100;
	alus[17].maxpadit=10;
	alus[17].patinopeus=0.5f;
	alus[17].patisade=0.6;
	alus[17].damage=2;
	alus[17].energia=15;
	alus[17].patipotkasu=0.01;
	alus[17].jalkikoko=0.12;
	alus[17].fuel=30;
	alus[17].tankkaus=0.04;
	alus[17].patifuel=2.9;
	alus[17].easelaji=17;
	alus[17].kaantonopeus=1.6f;
	alus[17].easefuel=3;
	alus[17].easekesto=300;
	alus[17].easenopeus=10;
	alus[17].easevauhti=1.2;
	alus[17].maxepadit=100;
	alus[17].edamage=3;
	alus[17].epatisade=2;
	alus[17].epatipotkasu=0;
	alus[17].patihajottaa=true;
	alus[17].epatihajottaa=true;
	alus[17].patiVoiHajota=true;
	alus[17].epatiVoiHajota=true;
	alus[17].patienergia=6; 
	alus[17].epatienergia=7;
	alus[17].pistemaara=14;

	
	if(!dserver){
	alus[17].ampumisaani=new Sound("sounds\\a_17_ampumis.wav");
	alus[17].eampumisaani=new Sound("sounds\\a_17_eampumis.wav");
	alus[17].osumisaani=new Sound("sounds\\a_17_osumis.wav");
	alus[17].eosumisaani=new Sound("sounds\\a_17_eosumis.wav");
	}


	sprintf(alus[18].nimi,"Ville");
	alus[18].numero=18;
	alus[18].paino=225;
	alus[18].nopeus=0.36;
	alus[18].sade=2.0f;
	alus[18].aselaji=18;
	alus[18].ampumisnopeus=20;
	alus[18].patiaika=2;
	alus[18].maxpadit=0;
	alus[18].patinopeus=0;
	alus[18].patisade=16;
	alus[18].damage=5;
	alus[18].energia=24;
	alus[18].patipotkasu=0;
	alus[18].jalkikoko=0.15;
	alus[18].fuel=35;
	alus[18].tankkaus=0;
	alus[18].patifuel=10;
	alus[18].easelaji=18;
	alus[18].kaantonopeus=1.4f;
	alus[18].easefuel=0;
	alus[18].easekesto=300;
	alus[18].easenopeus=10;
	alus[18].easevauhti=0;
	alus[18].maxepadit=100;
	alus[18].edamage=0;
	alus[18].epatisade=8;
	alus[18].epatipotkasu=0;
	alus[18].patihajottaa=false;
	alus[18].epatihajottaa=false;
	alus[18].patiVoiHajota=false;
	alus[18].epatiVoiHajota=false;
	alus[18].patienergia=0; 
	alus[18].epatienergia=0;
	alus[18].pistemaara=18;

	if(!dserver){
	alus[18].ampumisaani=new Sound("sounds\\a_18_ampumis.wav");
	alus[18].eampumisaani=new Sound("sounds\\a_18_eampumis.wav");
	alus[18].osumisaani=new Sound("sounds\\a_17_osumis.wav");
	alus[18].eosumisaani=new Sound("sounds\\a_17_eosumis.wav");
	}


	sprintf(alus[19].nimi,"Ville");
	alus[19].numero=19;
	alus[19].paino=275;
	alus[19].nopeus=0.0022;
	alus[19].sade=2.6f;
	alus[19].aselaji=19;
	alus[19].ampumisnopeus=15;
	alus[19].patiaika=100;
	alus[19].maxpadit=50;
	alus[19].patinopeus=0.6;
	alus[19].patisade=0.5;
	alus[19].damage=1;
	alus[19].energia=28;
	alus[19].patipotkasu=0;
	alus[19].jalkikoko=0.16;
	alus[19].fuel=30;
	alus[19].tankkaus=0.04;
	alus[19].patifuel=1;
	alus[19].easelaji=19;
	alus[19].kaantonopeus=1.5f;
	alus[19].easefuel=13;
	alus[19].easekesto=100;
	alus[19].easenopeus=110;
	alus[19].easevauhti=0;
	alus[19].maxepadit=100;
	alus[19].edamage=20;
	alus[19].epatisade=4;
	alus[19].epatipotkasu=0;
	alus[19].patihajottaa=true;
	alus[19].epatihajottaa=false;
	alus[19].patiVoiHajota=true;
	alus[19].epatiVoiHajota=false;
	alus[19].patienergia=5; 
	alus[19].epatienergia=0;
	alus[19].pistemaara=17;

	
	if(!dserver){
	alus[19].ampumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[19].eampumisaani=new Sound("sounds\\a_17_ampumis.wav");
	alus[19].osumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[19].eosumisaani=new Sound("sounds\\a_1_osumis.wav");
	}
	
	sprintf(alus[20].nimi,"Ville");
	alus[20].numero=20;
	alus[20].paino=390;
	alus[20].nopeus=0.0018;
	alus[20].sade=4.4f;
	alus[20].aselaji=20;
	alus[20].ampumisnopeus=30;
	alus[20].patiaika=120;
	alus[20].maxpadit=20;
	alus[20].patinopeus=0.6;
	alus[20].patisade=1.0;
	alus[20].damage=5;
	alus[20].energia=54;
	alus[20].patipotkasu=0;
	alus[20].jalkikoko=0.17;
	alus[20].fuel=40;
	alus[20].tankkaus=0.07;
	alus[20].patifuel=4;
	alus[20].easelaji=20;
	alus[20].kaantonopeus=1.5f;
	alus[20].easefuel=13;
	alus[20].easekesto=100;
	alus[20].easenopeus=50;
	alus[20].easevauhti=0.7;
	alus[20].maxepadit=50;
	alus[20].edamage=2;
	alus[20].epatisade=0.6;
	alus[20].epatipotkasu=0;
	alus[20].patihajottaa=true;
	alus[20].epatihajottaa=true;
	alus[20].patiVoiHajota=true;
	alus[20].epatiVoiHajota=true;
	alus[20].patienergia=10; 
	alus[20].epatienergia=3;
	alus[20].pistemaara=31;

	
	if(!dserver){
	alus[20].ampumisaani=new Sound("sounds\\a_20_ampumis.wav");
	alus[20].eampumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[20].osumisaani=new Sound("sounds\\a_1_osumis.wav");
	alus[20].eosumisaani=new Sound("sounds\\a_0_ampumis.wav");
	}
	
	
	sprintf(alus[21].nimi,"Ville");
	alus[21].numero=21;
	alus[21].paino=175;
	alus[21].nopeus=0.0022;
	alus[21].sade=3.4f;
	alus[21].aselaji=21;
	alus[21].ampumisnopeus=40;
	alus[21].patiaika=80;
	alus[21].maxpadit=200;
	alus[21].patinopeus=0.6;
	alus[21].patisade=0.44;
	alus[21].damage=1.5;
	alus[21].energia=28;
	alus[21].patipotkasu=0;
	alus[21].jalkikoko=0.14;
	alus[21].fuel=40;
	alus[21].tankkaus=0.02;
	alus[21].patifuel=6;
	alus[21].easelaji=21;
	alus[21].kaantonopeus=1.8f;
	alus[21].easefuel=5;
	alus[21].easekesto=100;
	alus[21].easenopeus=50;
	alus[21].easevauhti=0.7;
	alus[21].maxepadit=200;
	alus[21].edamage=2.5;
	alus[21].epatisade=0.5;
	alus[21].epatipotkasu=0;
	alus[21].patihajottaa=true;
	alus[21].epatihajottaa=true;
	alus[21].patiVoiHajota=true;
	alus[21].epatiVoiHajota=true;
	alus[21].patienergia=4; 
	alus[21].epatienergia=6;
	alus[21].pistemaara=18;

	
	if(!dserver){
	alus[21].ampumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[21].eampumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[21].osumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[21].eosumisaani=new Sound("sounds\\a_0_ampumis.wav");
	}

	sprintf(alus[22].nimi,"Ville");
	alus[22].numero=22;
	alus[22].paino=210;
	alus[22].nopeus=0.17;
	alus[22].sade=4.0f;
	alus[22].aselaji=22;
	alus[22].ampumisnopeus=30;
	alus[22].patiaika=400;
	alus[22].maxpadit=200;
	alus[22].patinopeus=0.7;
	alus[22].patisade=1.0;
	alus[22].damage=4.5;
	alus[22].energia=28;
	alus[22].patipotkasu=0;
	alus[22].jalkikoko=0.14;
	alus[22].fuel=60;
	alus[22].tankkaus=0.05;
	alus[22].patifuel=6;
	alus[22].easelaji=22;
	alus[22].kaantonopeus=4.5;
	alus[22].easefuel=9;
	alus[22].easekesto=80;
	alus[22].easenopeus=25;
	alus[22].easevauhti=0.6;
	alus[22].maxepadit=200;
	alus[22].edamage=1.5;
	alus[22].epatisade=0.4;
	alus[22].epatipotkasu=0;
	alus[22].patihajottaa=true;
	alus[22].epatihajottaa=true;
	alus[22].patiVoiHajota=true;
	alus[22].epatiVoiHajota=true;
	alus[22].patienergia=20; 
	alus[22].epatienergia=3;
	alus[22].pistemaara=22;

	
	if(!dserver){
	alus[22].ampumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[22].eampumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[22].osumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[22].eosumisaani=new Sound("sounds\\a_0_ampumis.wav");
	}

	sprintf(alus[23].nimi,"ville");
	alus[23].numero=23;
	alus[23].paino=240;
	alus[23].nopeus=0.0025;
	alus[23].sade=3.9f;
	alus[23].aselaji=23;
	alus[23].ampumisnopeus=3;
	alus[23].patiaika=800;
	alus[23].maxpadit=100;
	alus[23].patinopeus=0.65;
	alus[23].patisade=0.5;
	alus[23].damage=1.5;
	alus[23].energia=24;
	alus[23].patipotkasu=0.2;
	alus[23].jalkikoko=0.16;
	alus[23].fuel=40;
	alus[23].tankkaus=0.035;
	alus[23].patifuel=3;
	alus[23].easelaji=23;
	alus[23].kaantonopeus=1.7;
	alus[23].easefuel=38;
	alus[23].easekesto=200;
	alus[23].easenopeus=100;
	alus[23].easevauhti=0;
	alus[23].maxepadit=2;
	alus[23].edamage=85;
	alus[23].epatisade=1.0;
	alus[23].epatipotkasu=0;
	alus[23].patihajottaa=true;
	alus[23].epatihajottaa=false;
	alus[23].patiVoiHajota=true;
	alus[23].epatiVoiHajota=false;
	alus[23].patienergia=16; 
	alus[23].epatienergia=3;
	alus[23].pistemaara=24;

	
	if(!dserver){
	alus[23].ampumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[23].eampumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[23].osumisaani=new Sound("sounds\\a_0_ampumis.wav");
	alus[23].eosumisaani=new Sound("sounds\\a_0_ampumis.wav");
	}

	if(!dserver){
		setSoundVolumes();	
	}

	int i;

	maksimipadit=0;
	maksimiepadit=0;
	for(i=0;i<aluksia;i++){
		if(alus[i].maxpadit>maksimipadit) maksimipadit=alus[i].maxpadit;
		if(alus[i].maxepadit>maksimiepadit) maksimiepadit=alus[i].maxepadit;
	}
	
	netpatilahetetaan=(char*)malloc(sizeof(char)*maksimipadit*MAXNETPLAYERS);
	netepatilahetetaan=(char*)malloc(sizeof(char)*maksimipadit*MAXNETPLAYERS);
	for(i=0;i<maksimipadit*MAXNETPLAYERS;i++){
		netpatilahetetaan[i]=0;
		netepatilahetetaan[i]=0;
	}

	//maksimipadit=10000;
	//maksimiepadit=10000;

	for(i=0;i<players;i++){
		player[i].numero=i;
		player[i].kuolema=0;
		player[i].pylos=false;
		player[i].pfire=false;
		player[i].pease=false;
		player[i].palas=false;
		player[i].pleft=false;
		player[i].pright=false;
		player[i].ihmispelaaja=sets->ihminen[i];
		player[i].tiimi=sets->team[i];
	}


	pikkumies=new Pati[pikkumiehia];
	for(i=0;i<pikkumiehia;i++){
		pikkumies[i].aika=0;
	}

/*	for(i=0;i<players;i++){
		double temp3=random(10000);
		int temp3int=(int)(temp3/10001.0f*(double)aluksia);
		if(temp3int>=aluksia)temp3int-=aluksia;
		player[i].uudestaan(alus[temp3int]);
	}*/
	//T
	for(i=0;i<10;i++){
		star[i].x=i-60.0f;
		star[i].y=50.0f;
		star[i].z=-10.0f;
		star[i].r=random(100)/1000.0f+0.6f;
		star[i].g=random(100)/1000.0f+0.6f;
		star[i].b=star[i].g;
		star[i].koko=0.3;
	}
	for(i=0;i<10;i++){
		star[i+9].x=-56.0f;
		star[i+9].y=40+i;
		star[i+9].z=-10.0f;
		star[i+9].r=random(100)/1000.0f+0.6f;
		star[i+9].g=random(100)/1000.0f+0.6f;
		star[i+9].b=star[i].g;
		star[i+9].koko=0.3;
	}
	//H
	for(i=0;i<11;i++){
		star[i+20].x=-50.0f;
		star[i+20].y=i+40.0f;
		star[i+20].z=-10.0f;
		star[i+20].r=random(100)/1000.0f+0.6f;
		star[i+20].g=random(100)/1000.0f+0.6f;
		star[i+20].b=star[i].g;
		star[i+20].koko=0.3;
	}
	for(i=0;i<11;i++){
		star[i+31].x=-41.0f;
		star[i+31].y=i+40.0f;
		star[i+31].z=-10.0f;
		star[i+31].r=random(100)/1000.0f+0.6f;
		star[i+31].g=random(100)/1000.0f+0.6f;
		star[i+31].b=star[i].g;
		star[i+31].koko=0.3;
	}
	for(i=0;i<8;i++){
		star[i+42].x=-49.0f+i;
		star[i+42].y=45.0f;
		star[i+42].z=-10.0f;
		star[i+42].r=random(100)/1000.0f+0.6f;
		star[i+42].g=random(100)/1000.0f+0.6f;
		star[i+42].b=star[i].g;
		star[i+42].koko=0.3;
	}

	//P
	for(i=0;i<11;i++){
		star[i+50].x=-38.0f;
		star[i+50].y=i+40;
		star[i+50].z=-10.0f;
		star[i+50].r=random(100)/1000.0f+0.6f;
		star[i+50].g=random(100)/1000.0f+0.6f;
		star[i+50].b=star[i].g;
		star[i+50].koko=0.3;
	}

	for(i=0;i<5;i++){
		star[i+61].x=-37.0f+i;
		star[i+61].y=50.0f;
		star[i+61].z=-10.0f;
		star[i+61].r=random(100)/1000.0f+0.6f;
		star[i+61].g=random(100)/1000.0f+0.6f;
		star[i+61].b=star[i].g;
		star[i+61].koko=0.3;
	}
	for(i=0;i<5;i++){
		star[i+66].x=-37.0f+i;
		star[i+66].y=45.0f;
		star[i+66].z=-10.0f;
		star[i+66].r=random(100)/1000.0f+0.6f;
		star[i+66].g=random(100)/1000.0f+0.6f;
		star[i+66].b=star[i].g;
		star[i+66].koko=0.3;
	}
	for(i=0;i<4;i++){
		star[i+71].x=-33.0f;
		star[i+71].y=i+46;
		star[i+71].z=-10.0f;
		star[i+71].r=random(100)/1000.0f+0.6f;
		star[i+71].g=random(100)/1000.0f+0.6f;
		star[i+71].b=star[i].g;
		star[i+71].koko=0.3;
	}
//19,18
//	star[19].koko=0.3;
	for(i=75;i<stars;i++){
		star[i].x=random(maxx*8)-maxx*4;
		star[i].y=random(maxy*8)-maxy*4;
		star[i].z=random(10000)/200.0f*(-1.0f)-180.0f;
		star[i].xv=0;
		star[i].yv=0;
		star[i].xv=0;
		star[i].r=random(100)/1000.0f+0.9f;
		star[i].g=random(250)/1000.0f+0.75f;
		star[i].b=star[i].g;
		star[i].koko=1.0;
	}
	asteroidi=new Pati[asteroideja];
	for(i=0;i<asteroideja;i++){
		asteroidi[i].koko=random(50)/100.0f+0.8f;
		asteroidi[i].alku=random(10000)/10.0f;
		asteroidi[i].r=random(300)/100.0f-1.5f;
		asteroidi[i].g=random(300)/100.0f-1.5f;
		asteroidi[i].b=random(300)/100.0f-1.5f;
		asteroidi[i].energia=asteroidienergia;
		double raja=maxx/4.0f;
		double raja2=maxx*2;
		asteroidi[i].x=raja-1;
		asteroidi[i].y=raja-1;
		while(fabs(asteroidi[i].x)<=raja && fabs(asteroidi[i].y)<=raja){
			asteroidi[i].x=random(raja2*2)-raja2;
			asteroidi[i].y=random(raja2*2)-raja2;
		}
		asteroidi[i].xv=asteroidi[i].x;
		asteroidi[i].yv=asteroidi[i].y;
		while(fabs(atan2(asteroidi[i].yv,asteroidi[i].xv)
				  -atan2(asteroidi[i].y,asteroidi[i].x))
				  <PI/6
				  || sqrt(asteroidi[i].xv*asteroidi[i].xv+asteroidi[i].yv*asteroidi[i].yv)
				     >0.5){
			asteroidi[i].xv=random(1000)/2000.0f-0.25f;
			asteroidi[i].yv=random(1000)/2000.0f-0.25f;
		}
	}
	if(!dserver)
		setLists();

	alustaDPisteet();
	alustaCPisteet();
	alustaSPisteet();
	for(i=0;i<10;i++){
		siirrot[i].aika=0;
	}
}

void setLists(){
	glGenLists(50);
	GLUquadricObj *gah;
	GLUquadricObj *temp;
	list_asteroid=1;
	glNewList(list_asteroid, GL_COMPILE);
		gah=gluNewQuadric();
		gluQuadricTexture(gah,true);
		gluSphere(gah, 1, 8+sets->detail*4, 7+sets->detail*4);
		gluDeleteQuadric(gah);
	glEndList();

	list_int=new unsigned int[100];
	int i;
	for(i=0;i<100;i++){
		list_int[i]=i+2;
	}

	//peruselementit
	glNewList(list_int[0], GL_COMPILE);
		drawCube();
	glEndList();

	glNewList(list_int[1], GL_COMPILE);
		drawTri();
	glEndList();

	//alukset
	alusPiirto();


	//kukat
	glNewList(KUKKA1, GL_COMPILE);
	  temp=gluNewQuadric();
	  glPushMatrix();
		glPushMatrix();
			glColor4f(0.1,0.8,0.1,0.5);
			glRotatef(-90,1,0,0);
			gluCylinder(temp,0.075,0.075,1,5+4*sets->detail,1+1*sets->detail);
		glPopMatrix();
		glTranslatef(0,1,0);
		glPushMatrix();
			glColor4f(0.1,0.1,0.8,0.5);
			glScalef(1,0.75,1);
			gluSphere(temp,0.25,8+6*sets->detail,5+5*sets->detail);
		glPopMatrix();
		glColor4f(0.7,0.3,0.3,0.5);
		for(i=0;i<6;i++){
			glPushMatrix();
				glRotatef(i*60,0,1,0);
				glTranslatef(0,0.05,-0.25);
				glRotatef(25,1,0,0);
				glScalef(0.5,0.25,1);
				gluSphere(temp,0.4,8+6*sets->detail,5+5*sets->detail);
			glPopMatrix();
		}
	  glPopMatrix();
	  gluDeleteQuadric(temp);
	glEndList();

	glNewList(KUKKA2, GL_COMPILE);
	  temp=gluNewQuadric();
	  glPushMatrix();
		glPushMatrix();
			glColor4f(0.1,0.8,0.1,0.5);
			glRotatef(-90,1,0,0);
			gluCylinder(temp,0.075,0.075,1,5+4*sets->detail,1+1*sets->detail);
		glPopMatrix();
		glTranslatef(0,1,0);
		glPushMatrix();
			glColor4f(0.8,0.1,0.1,0.5);
			glScalef(1,0.75,1);
			gluSphere(temp,0.25,8+6*sets->detail,5+5*sets->detail);
		glPopMatrix();
		glColor4f(0.3,0.3,0.7,0.5);
		for(i=0;i<6;i++){
			glPushMatrix();
				glRotatef(i*60,0,1,0);
				glTranslatef(0,0.05,-0.25);
				glRotatef(25,1,0,0);
				glScalef(0.5,0.25,1);
				gluSphere(temp,0.4,8+6*sets->detail,5+5*sets->detail);
			glPopMatrix();
		}
	  glPopMatrix();
	  gluDeleteQuadric(temp);
	glEndList();

	glNewList(KUKKA3, GL_COMPILE);
	  temp=gluNewQuadric();
	  glPushMatrix();
		glPushMatrix();
			glColor4f(0.1,0.8,0.1,0.5);
			glRotatef(-90,1,0,0);
			gluCylinder(temp,0.075,0.075,1,5+4*sets->detail,1+1*sets->detail);
		glPopMatrix();
		glTranslatef(0,1,0);
		glPushMatrix();
			glColor4f(0.8,0.1,0.8,0.5);
			glScalef(1,0.75,1);
			gluSphere(temp,0.25,8+6*sets->detail,5+4*sets->detail);
		glPopMatrix();
		glColor4f(0.3,0.7,0.3,0.5);
		for(i=0;i<6;i++){
			glPushMatrix();
				glRotatef(i*60,0,1,0);
				glTranslatef(0,0.05,-0.25);
				glRotatef(25,1,0,0);
				glScalef(0.5,0.25,1);
				gluSphere(temp,0.4,8+6*sets->detail,5+5*sets->detail);
			glPopMatrix();
		}
	  glPopMatrix();
	  gluDeleteQuadric(temp);
	glEndList();

	glNewList(KUKKA4, GL_COMPILE);
	  temp=gluNewQuadric();
	  glPushMatrix();
		glPushMatrix();
//			glColor4f(0.1,0.8,0.1,0.5);
			glRotatef(-90,1,0,0);
			gluCylinder(temp,0.075,0.075,1,5+4*sets->detail,1+1*sets->detail);
		glPopMatrix();
		glTranslatef(0,1,0);
		glPushMatrix();
//			glColor4f(0.8,0.1,0.8,0.5);
			glScalef(1,0.75,1);
			gluSphere(temp,0.25,8+6*sets->detail,5+5*sets->detail);
		glPopMatrix();
//		glColor4f(0.3,0.7,0.3,0.5);
		for(i=0;i<6;i++){
			glPushMatrix();
				glRotatef(i*60,0,1,0);
				glTranslatef(0,0.05,-0.25);
				glRotatef(25,1,0,0);
				glScalef(0.5,0.25,1);
				gluSphere(temp,0.4,8+6*sets->detail,5+4*sets->detail);
			glPopMatrix();
		}
	  glPopMatrix();
	  gluDeleteQuadric(temp);
	glEndList();

	glNewList(SIENI, GL_COMPILE);
	  glPushMatrix();
	        temp=gluNewQuadric();
		glPushMatrix();
			glColor4f(0.8,0.8,0.8,0.5);
			glRotatef(-90,1,0,0);
			gluCylinder(temp,0.1,0.1,1,7+5*sets->detail,1+1*sets->detail);
		glPopMatrix();
		glTranslatef(0,1,0);
		glPushMatrix();
			glColor4f(0.8,0.1,0.1,0.5);
			glScalef(1,0.25,1);
			gluSphere(temp,0.5,8+6*sets->detail,5+5*sets->detail);
		glPopMatrix();
		glPushMatrix();
			glColor4f(0.8,0.9,0.9,0.5);
			glTranslatef(0.25,0.09,-0.1);
			gluSphere(temp,0.08,4+2*sets->detail,3+2*sets->detail);
			glTranslatef(-0.1,0,0.35);
			gluSphere(temp,0.075,4+2*sets->detail,3+2*sets->detail);
			glTranslatef(-0.2,0,-0.1);
			gluSphere(temp,0.06,4+2*sets->detail,3+2*sets->detail);
			glTranslatef(0.05,0, -0.15);
			gluSphere(temp,0.07,4+2*sets->detail,3+2*sets->detail);
			glTranslatef(-0.15,0, -0.1);
			gluSphere(temp,0.075,4+2*sets->detail,3+2*sets->detail);

			glTranslatef(0.1,0,0.45);
			gluSphere(temp,0.075,4+2*sets->detail,3+2*sets->detail);
			glTranslatef(0,0,-0.7);
			gluSphere(temp,0.075,4+2*sets->detail,3+2*sets->detail);
		glPopMatrix();
		gluDeleteQuadric(temp);
	  glPopMatrix();
	glEndList();



	glNewList(NUOLI, GL_COMPILE);
	  temp=gluNewQuadric();
	  glPushMatrix();
			glColor4f(0.8,0.8,0.8,0.5);
			glScalef(2,0.4,0.2);
			gluSphere(temp,1,15,10);
			glScalef(1/2.0f,2.5f,1.5);
			glTranslatef(2,0,0);
			glRotatef(30,0,0,1);
			gluSphere(temp,1,3,15);
		  glPopMatrix();
		gluDeleteQuadric(temp);
	glEndList();

	nuolirot=0;



}

float nuolirot;
unsigned int list_asteroid;
unsigned int *list_int;

