#include "main.h"
#include "net.h"
#include "scene.h"
#include "glapi.h"
#include "netgamesets.h"
#include "netav.h"
#include "peliloppu.h"
#include "domination.h"
#include "ctf.h"
#include "sieni.h"
#include "nethelp.h"

int asteroidipaivitysaika=5;
int paivitettavaasteroidi=0;

asteroidipox prajahdys[MAXNETPLAYERS];
bool playerrajahtaa[MAXNETPLAYERS];
bool netGameQuit=false;
unsigned short netframe=0;
unsigned short netframe2=0;
SDLNet_SocketSet joinset;
SDLNet_SocketSet hostset;
int yhteyksia;
void playeritUudestaanJuttu(){
	int i;
			for(i=0;i<players;i++){
				if(playeruudestaan[i]==1 ){

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
				if((i<hosteja || mpstatus[i]==2) && playeruudestaan[i]==2){
					netValitseAlus(i,i);
					playeruudestaan[i]=0;
					player[i].laitaAlkuSirpaleet();
					player[i].kuolema--;
					playeruudestaan[i]=1;
				}
			}
			for(i=0;i<players;i++){
				if(playeruudestaan[i]==3){
					playeruudestaan[i]=1;
				}
			}
}

void netHostPeli(){
	int i;

	yhteyksia=0;
	hostset=SDLNet_AllocSocketSet(16);
	for(i=1;i<16;i++){
		if(tcpsoketit[i]!=NULL && tila[i]>0){
			int numused2=SDLNet_TCP_AddSocket(hostset,tcpsoketit[i]);
			if(i!=0) yhteyksia++;
			printf("soketti lis‰tty %i\n",i);
		}
	}


	pistepeli=false;
	hostpeli=true;
	//calculatefps=85;
	//calculatefps=100;
	netframe=0;
	pistepeli=false;
	for(i=0;i<players;i++){
		player[i].kuolema=0;
		playerrajahtaa[i]=false;
		if(gamemode==DEATHMATCHRANDOM){
			playeruudestaan[i]=1;
			player[i].pisteet=mpcredits;
			pistepeli=true;
		}
		else if(gamemode==DEATHMATCH){
			playeruudestaan[i]=2;
			player[i].pisteet=mpcredits;
			pistepeli=true;
		}else if(gamemode==DOMINATION){
			playeruudestaan[i]=1;
			player[i].pisteet=0;
		}else if(gamemode==CTF){
			playeruudestaan[i]=1;
			player[i].pisteet=0;
		}else if(gamemode==MUSHROOM){
			playeruudestaan[i]=1;
			player[i].pisteet=0;
		}
	}
	for(i=0;i<cpisteita;i++){
		cpiste[i].enabled=false;
		cpiste[i].x=alkupiste[i].x;
		cpiste[i].y=alkupiste[i].y;
		cpiste[i].player=100;
	}
	for(i=0;i<players;i++){
		cpiste[player[i].tiimi].enabled=true;
	}
	for(i=0;i<dpisteita;i++){
		dpiste[i].tiimi=-1;
	}
	for(i=0;i<spisteita;i++){
		spiste[i].x=0;
		spiste[i].y=50;
		spiste[i].uusiks=true;
	}
	for(i=0;i<players;i++){
		double temp3=random(10000);
		int temp3int=(int)(temp3/10001.0f*(double)aluksia);
		if(temp3int>=aluksia)temp3int-=aluksia;
		player[i].uudestaan(alus[temp3int]);
	}
	//SDL_Delay(1500);
	bool ekaNetFrame=true;
	int piirrettyframe=0;
	while(!netGameQuit){

		hostKuuntelePeli();
		currenttime=SDL_GetTicks()-starttime;
		framecounter=calculatefps*currenttime/1000.0;
		bool drawn=false;
		while (oldframecounter==framecounter){
			if(!drawn){
				drawedframes++;
//				printf("drawalku:%i\n",SDL_GetTicks());
				piirrettyframe=framecounter;
				drawFrame(framecounter);
//				printf("drawloppu:%i\n",SDL_GetTicks());
				drawn=true;
			}else{
				SDL_Delay(1);
			}
			currenttime=SDL_GetTicks()-starttime;
			framecounter=calculatefps*currenttime/1000.0;
		}
		//printf("piir=%i,fr=%i\n",piirrettyframe,framecounter);
		if(!drawn && piirrettyframe<framecounter-100){
				drawedframes++;
				piirrettyframe=framecounter;
				drawFrame(framecounter);
				drawn=true;
		}
		skipframes=framecounter-oldframecounter;
		if(skipframes>10) skipframes=10;


		for (;skipframes>0;skipframes--){
			if(!ekaNetFrame){
				processEvents();
				hostKuuntelePeli();
				netframe++;
				calculatedframes++;
				calculateFrame(++oldframecounter);			
			}
			ekaNetFrame=false;
			playeritUudestaanJuttu();
			hostLahetaPeli();

		}

		processEvents();
		updateAudio();
		if(escpressed) netGameSets(true);
		if(peliloppu()){
			netGameQuit=true;
			netVictory();
		}
	}
/*	while(!netGameQuit){
		do{
			currenttime=SDL_GetTicks()-starttime;
			framecounter=calculatefps*currenttime/1000.0;
		}	while (oldframecounter==framecounter);
		if(framecounter-oldframecounter>10) oldframecounter=framecounter-1;
		skipframes=framecounter-oldframecounter;
		for (;skipframes>0;skipframes--){
			if(!ekaNetFrame){
				
				hostKuuntelePeli();
				netframe++;
				calculatedframes++;
				calculateFrame(++oldframecounter);			
			}
			ekaNetFrame=false;
			playeritUudestaanJuttu();
			hostLahetaPeli();

		}
		processEvents();
				drawedframes++;
		drawFrame(framecounter);
		framesdrawn++;
		updateAudio();
		if(escpressed) netGameSets(true);
		if(peliloppu()){
			netGameQuit=true;
			netVictory();
		}
	}*/
	netGameQuit=false;
	SDLNet_FreeSocketSet(hostset);

}

bool asteroidirajahtaa=false;
void netJoinPeli(){
	joinset=SDLNet_AllocSocketSet(1);
	int numused=SDLNet_TCP_AddSocket(joinset,joinsoketti);
	netframe2=0;
	netframe=0;
	int i,j;
	joinpeli=true;
	pistepeli=false;
	//calculatefps=85;
	//calculatefps=100;
	if(gamemode==DEATHMATCH || gamemode==DEATHMATCHRANDOM){
			pistepeli=true;
	}else{
			pistepeli=false;
	}
	for(i=0;i<players;i++){
		player[i].kuolema=0;
	}
	for(i=0;i<players;i++){
		for(j=0;j<maksimipadit;j++){
			player[i].pati[j].aika=0;
		}
		for(j=0;j<maksimiepadit;j++){
			player[i].epati[j].aika=0;
		}
/*		double temp3=random(10000);
		int temp3int=(int)(temp3/10001.0f*(double)aluksia);
		if(temp3int>=aluksia)temp3int-=aluksia;
		player[i].uudestaan(alus[temp3int]);*/
	}
	for(i=0;i<joinaavia;i++){
		player[i].palas=false;
		player[i].pylos=false;
		player[i].pleft=false;
		player[i].pright=false;
		player[i].pfire=false;
		player[i].pease=false;
	}
	for(i=0;i<cpisteita;i++){
		cpiste[i].enabled=false;
		cpiste[i].x=alkupiste[i].x;
		cpiste[i].y=alkupiste[i].y;
		cpiste[i].player=100;
	}
	for(i=0;i<players;i++){
		cpiste[player[i].tiimi].enabled=true;
	}
	for(i=0;i<spisteita;i++){
		spiste[i].x=0;
		spiste[i].y=50;
	}
	//SDL_Delay(1500);
	
	//char netframe2=0;

	/*while(!netGameQuit){
		do{
			currenttime=SDL_GetTicks()-starttime;
			framecounter=calculatefps*currenttime/1000.0;
		}	while (oldframecounter==framecounter);
		if(framecounter-oldframecounter>10) oldframecounter=framecounter-1;
		skipframes=framecounter-oldframecounter;
		for (;skipframes>0;skipframes--){

			joinPeliNetwork();

			for(i=0;i<players;i++){
				if(playeruudestaan[i]==2){
					playeruudestaan[i]=0;
					printf("valitaan alus pelaajalle l=%i, n=%i\n",i,pelaajanumero[i]);
					netValitseAlus(i,pelaajanumero[i]);
				}
			}
					calculatedframes++;
			calculateFrame(++oldframecounter);
		}
		processEvents();
				drawedframes++;
		drawFrame(framecounter);
		framesdrawn++;
		updateAudio();
		if(escpressed) netGameSets(true);
//		if(escpressed)netGameQuit=true;
	}*/
	int piirrettyfreimi=0;
	while(!netGameQuit){
		joinKuuntelePeli();
//		if(netframe2>netframe) netframe2=0;
		bool drawn=false;
		if(!netGameQuit) while(netframe==netframe2) {
			if(!drawn){
				drawedframes++;
				piirrettyfreimi=netframe;
//				printf("drawalku:%i\n",SDL_GetTicks());
				drawFrame(framecounter);
//				printf("drawloppu:%i\n",SDL_GetTicks());
				drawn=true;
			}else{
				SDL_Delay(2);
			}
			joinKuuntelePeli();
			if(netframe2>netframe) netframe2=0;
			if(netGameQuit) break;
		}
		if(!drawn && piirrettyfreimi<netframe-5){
			piirrettyfreimi=netframe;
			drawFrame(framecounter);
			drawedframes++;
			drawn=true;
		}
		skipframes=netframe-netframe2;
		//printf("net=%i,skip=%i,aika=%i\n",netframe,skipframes,SDL_GetTicks());
		//netframe2=netframe;
		if(skipframes>20) skipframes=20;
		netframe2+=skipframes;
			for(i=0;i<players;i++){
				if(playeruudestaan[i]==2){
					playeruudestaan[i]=0;
					printf("valitaan alus pelaajalle l=%i, n=%i\n",i,pelaajanumero[i]);
					netValitseAlus(i,pelaajanumero[i]);
				}
			}
		if(!netGameQuit) for(;skipframes>0;skipframes--){
			processEvents();
			lahetaNapitHostille();
			//joinKuuntelePeli();
			calculatedframes++;
			calculateFrame(++oldframecounter);
		}
		framecounter=oldframecounter;

		processEvents();
		framesdrawn++;
		updateAudio();
		if(escpressed) netGameSets(true);
	}
	//hostQuit();
	netGameQuit=false;
	SDLNet_FreeSocketSet(joinset);
}


void hostPeliNetwork(){
}

void joinPeliNetwork(){
	joinKuuntelePeli();
	lahetaNapitHostille();
}

void hostKuuntelePeli(){
	int numready=0,i,numero=-1;
	if(SDL_GetTicks() >= (unsigned int)(timeouttarkistettu+timeoutaika)){
		timeouttarkistettu=SDL_GetTicks();
		for(i=1;i<MAXNETPLAYERS;i++) {
			if(tila[i]>0){
				timeout[i]--;
				if(timeout[i]<1){
					int numused=SDLNet_TCP_DelSocket(hostset,tcpsoketit[i]);
					yhteyksia--;
					disconnect(i);
				}
			}
		}
	}

	//int yhteyksia=0;
	//SDLNet_SocketSet set=SDLNet_AllocSocketSet(16);
/*	for(i=0;i<16;i++){
		if(tcpsoketit[i]!=NULL && tila[i]>0){
			int numused=SDLNet_TCP_AddSocket(set,tcpsoketit[i]);
			if(i!=0) yhteyksia++;
		}
	}*/

	if(yhteyksia>0) numready=SDLNet_CheckSockets(hostset, 0);
//	if(yhteyksia>0) numready=SDLNet_CheckSockets(set, 0);
	if(numready==-1) {
		printf("SDLNet_CheckSockets: %s\n", SDLNet_GetError());
		perror("SDLNet_CheckSockets");
	}
	else if(numready) {
		//ei haluta lis‰‰ kliettej‰ en‰‰..
/*		if(SDLNet_SocketReady(tcpsoketit[0])) {
			TCPsocket client=SDLNet_TCP_Accept(tcpsoketit[0]);
			soketteja++;
			numero=addreadysocket(client);
			printf("Tulipa yksi soketti taas lis‰tty‰\n");
			int numused=SDLNet_TCP_AddSocket(set,tcpsoketit[numero]);
		}*/
	}
	if(numready) for(i=1;i<MAXNETPLAYERS;i++){
		if(tcpsoketit[i]!=NULL && tila[i]>=0 && i!=numero){
//			printf("kuunnellaan sokettia %i\n",i);
			if(SDLNet_SocketReady(tcpsoketit[i])!=0){
				int result,j;
				char msg[MAXLEN];
				for(j=0;j<MAXLEN;j++) msg[j]=0;
				result=SDLNet_TCP_Recv(tcpsoketit[i],msg,MAXLEN);
				if(result<0) {
					printf("jotain hassua p‰‰si k‰ym‰‰n, tuli %i byte‰ soketilta %i.\n",result,i);// An error may have occured, but sometimes you can just ignore it
					int numused=SDLNet_TCP_DelSocket(hostset,tcpsoketit[i]);
					yhteyksia--;
					disconnect(i);
				}
				else if(result>0){
					hostKasittele(msg, result, i);
					timeout[i]=TIME_OUT_MAARA;
					//paivitaPelaajat();
				}
				else printf("nollaa byte‰ sataa\n");
			}
		}
	}
//	SDLNet_FreeSocketSet(set);
}

void hostKasittele(char *msg, int result, int i){
	int j,n=0;
	if(compare(msg,"nap",3)){
		char luku=msg[3];
		int maara=luku;
		if(maara>pelaajiasoketista[i])maara=pelaajiasoketista[i];
		int k=0;
		n=4;
		for(j=0;j<maara;j++){
			int laskuri;
			for(laskuri=k;laskuri<playerslots;laskuri++){
				if(pelaajasoketista[laskuri]==i){
					k=laskuri;
					laskuri=playerslots;
				}
			}
/*			napit nappeja;
			memcpy(&nappeja,&msg[8+j*sizeof(napit)],sizeof(napit));
			player[k].pylos=(bool)nappeja.ylos;
			player[k].palas=(bool)nappeja.alas;
			player[k].pleft=(bool)nappeja.left;
			player[k].pright=(bool)nappeja.right;
			player[k].pfire=(bool)nappeja.fire;
			player[k].pease=(bool)nappeja.ease;
			k++;*/
			unsigned char napit=msg[n];
			n++;
			player[k].pylos=false;
			player[k].palas=false;
			player[k].pleft=false;
			player[k].pright=false;
			player[k].pfire=false;
			player[k].pease=false;

			if(napit&64) player[k].pylos=true;
			if(napit&2) player[k].palas=true;
			if(napit&4) player[k].pleft=true;
			if(napit&8) player[k].pright=true;
			if(napit&16) player[k].pfire=true;
			if(napit&32) player[k].pease=true;
			k++;
		}
		//n=8+maara*sizeof(napit);
	}else if(compare(msg,"tiimit saatu", 10)){
		char *msg2="kaikki kunnossa;";
		int len2=strlen(msg2)+1;
		int j2;
		int i2;
		for(j2=0;j2<1;j2++){
			for(i2=1;i2<playerslots;i2++){
				if(tcpsoketit[i2]!=NULL && tila[i2]>0){
					result=SDLNet_TCP_Send(tcpsoketit[i],msg2,len2);
					if(result<len2) {
						printf("SDLNet_TCP_Send: %s\n", SDLNet_GetError());
					}else{
						printf("sanottiin %i:lle:\"%s\"",i2,msg2);
					}
				}
			}
			SDL_Delay(2);
		}
		n=12;
	}else if(compare(msg,"alusv",5)){
		char pelaaja=msg[5];
		char alus2=msg[6];
		printf("clientti %i, pelaaja %i, alus %i\n",i,pelaaja,alus);
		if(pelaajasoketista[pelaaja]==i && player[pelaaja].pisteet>=alus[alus2].pistemaara){
			player[pelaaja].uudestaan(alus[alus2]);
			player[pelaaja].laitaAlkuSirpaleet();
			player[pelaaja].kuolema--;
			playeruudestaan[pelaaja]=3;
		}else if(pelaajasoketista[pelaaja]==i){
			playeruudestaan[pelaaja]=2;
		}
		n=7;
	}
	int i2;
	if(result>n+4){
		for(i2=0;i2<4;i2++){
			if(msg[n]==';'){
				n++;
				if(!(msg[n]<='z' && msg[n]>='a'))n++;
				if(!(msg[n]<='z' && msg[n]>='a'))n++;
				if(!(msg[n]<='z' && msg[n]>='a'))n++;
				if(result-n>5){
					hostKasittele(&msg[n], result-n,i);
					i2=6;
				}
			}
			n++;
		}
	}

	//printf("Kuuntelija, Received: \"%s\" from socket:%i\n",msg,i);
}

void hostLahetaPeli(){
	int i;
	unsigned char maara;
	char viesti[MAXLEN];
	sprintf(viesti,"pal");	//palku
	viesti[3]=(char)(players);	//players
	int n=4;
//	if(player[0].ease) printf("player %i ease=1\n",0);
//	else printf("player %i ease=0\n",i);

	//l‰hetet‰‰n kaikkien koordinaatit
	for(i=0;i<players;i++){
		n+=kirjotaCoords(&viesti[n],&player[i]);
	}
//	memcpy(&viesti[n],&netframe,2);
//	n+=2;
	//viesti[n]=netframe;



	/* updeitataan tasaisesti asteroideja.. */
	if(asteroidipaivitysaika<=0){
		asteroidipaivitysaika=2;
		asteroidiuudestaan[paivitettavaasteroidi]=true;
		paivitettavaasteroidi++;
		if(paivitettavaasteroidi>=asteroideja) paivitettavaasteroidi=0;
	}else asteroidipaivitysaika--;

	maara=0;	
	for(i=0;i<asteroideja;i++){ //uudestaan pistett‰v‰t asteroidit
		if(asteroidiuudestaan[i])maara++;
	}
	viesti[n]=(char)maara;
	n++;
	for(i=0;i<asteroideja;i++){
		if(asteroidiuudestaan[i]){
			viesti[n]=(char)i;
			n++;
			//n+=kirjotaPati(&viesti[n],&asteroidi[i]);
			n+=kirjotaAsteroidi(&viesti[n],&asteroidi[i]);
			//memcpy(&viesti[n],&asteroidi[i],sizeof(Pati));
			//n+=sizeof(Pati);
			asteroidiuudestaan[i]=false;
		}
	}

	/* asteroidir‰j‰hdykset (v‰hiten merkitsev‰ bitti) +
	   pelaajar‰j‰hdykset (shiftattuna vasemmalle kahdella) +
	   pelaajien uudelleen laittamiset, jos tluku&2 */

	maara=0;
	for(i=0;i<players;i++){//pelaajarajahdyksia
		if(playerrajahtaa[i]){
			maara++;
		}
	}
	unsigned char tluku=((char)maara << 2);
	if(asteroidirajahtaa) tluku++;

	maara=0;
	for(i=0;i<players;i++){ //uudestaan pistett‰v‰t pelaajat
		if(playeruudestaan[i]==1)maara++;
	}
	int umaara=0;
	if(maara>0){
		tluku+=2;
		umaara=maara;
	}
	viesti[n]=tluku;
	n++;

	if(asteroidirajahtaa){
		n+=kirjotaAsteroidipox(&viesti[n],&arajahdys);
		asteroidirajahtaa=false;
	}

	for(i=0;i<players;i++){
		if(playerrajahtaa[i]){
			playerrajahtaa[i]=false;
			viesti[n]=(char)i;
			n++;
			//memcpy(&viesti[n],&prajahdys[i],sizeof(asteroidipox));
			//n+=sizeof(asteroidipox);
			n+=kirjotaAsteroidipox(&viesti[n],&prajahdys[i]);
		}
	}

	maara=0;
	if(umaara>0){//uudestaan pistett‰v‰t pelaajat mahd jatkuu
		viesti[n]=umaara;
		n++;
		for(i=0;i<players;i++){
			if(playeruudestaan[i]==1){
				printf("uudestaan pelaaja %i\n",i);
				viesti[n]=(char)i;
				n++;
				viesti[n]=(char)player[i].alus.numero;
				n++;
				//if(i>=hosteja && mpstatus[i]!=2) 
				playeruudestaan[i]=0;
			}
		}
	}


	/* end of asteroidir‰j‰hdykset & pelaajat.. */

	//uusia pateja
	int patimaara=0,patimaara1=0,patimaara2=0;//0=pateja yht, 1=syntyvat, 2=kuolevat
	int j;
	for(i=0;i<players;i++){
		for(j=0;j<player[i].alus.maxpadit;j++)if(patimaara<MAXPATIMAARA){
			int kohta=i*maksimipadit+j;
			if(netpatilahetetaan[kohta]==0){
				if(player[i].pati[j].aika>0){
					netpatilahetetaan[kohta]=1;
					patimaara++;
					patimaara1++;
				}
			}else if(netpatilahetetaan[kohta]==2){
				if(player[i].pati[j].aika<=0){
					netpatilahetetaan[kohta]=3;
					patimaara++;
					patimaara2++;
				}
			}
		}
	}
	netPati netpati;
	viesti[n]=(char)patimaara1;
	n++;
	maara=0;
	for(i=0;i<players;i++){
		for(j=0;j<player[i].alus.maxpadit;j++){
			int kohta=i*maksimipadit+j;
			if(netpatilahetetaan[kohta]==1 && maara<patimaara1){
				netpatilahetetaan[kohta]=2;
				netpati.kulma=player[i].pati[j].kulma;
				netpati.x=player[i].pati[j].x;
				netpati.y=player[i].pati[j].y;
				netpati.xv=player[i].pati[j].xv;
				netpati.yv=player[i].pati[j].yv;
				netpati.r=player[i].pati[j].r;//*256.0f;
				netpati.g=player[i].pati[j].g;//*256.0f;
				netpati.b=player[i].pati[j].b;//*256.0f;
				netpati.pati=j;
				netpati.pelaaja=(char)i;
				//memcpy(&viesti[n],&netpati,sizeof(netPati));
				//n+=sizeof(netPati);
				int juttu=0;
				juttu=kirjotaNetpati(&viesti[n],&netpati);
				n+=juttu;
				maara++;
//				printf("luotiin pati %i,%i\n",i,j);
			}
		}
	}
	//printf("lahetetaan padit: maara=%i\n",maara);

	//laitetaan kuolleet padit
	viesti[n]=(char)patimaara2;
	n++;
	maara=0;
	kuolevaPati kuolevapati;
	for(i=0;i<players;i++){
		for(j=0;j<player[i].alus.maxpadit;j++){
			int kohta=i*maksimipadit+j;
/*			if(netpatilahetetaan[kohta]==3 && maara<patimaara2){
				netpatilahetetaan[kohta]=0;
				netpati.pelaaja=(char)i;
				netpati.pati=j;
				memcpy(&viesti[n],&netpati,sizeof(netPati));
				n+=sizeof(netPati);		
				maara++;
			}*/


			if(netpatilahetetaan[kohta]==3 && maara<patimaara2){
				netpatilahetetaan[kohta]=0;
				kuolevapati.pelaaja=(char)i;
				kuolevapati.pati=(short)j;
				//memcpy(&viesti[n],&kuolevapati,sizeof(kuolevaPati));
				//n+=sizeof(kuolevaPati);		
				int juttu=0;
				juttu=kirjotaKuolevapati(&viesti[n],&kuolevapati);
				n+=juttu;
//				n+=kirjotaKuolevapati(&viesti[n],&kuolevapati);
				maara++;
			}

		}
	}
	//printf("k‰skettiin tappaa %i patia\n",viesti[maarakohta]);


	//uudet epadit
	patimaara1=0;patimaara2=0;
	for(i=0;i<players;i++){
		for(j=0;j<player[i].alus.maxepadit;j++)if(patimaara<MAXPATIMAARA){
			int kohta=i*maksimiepadit+j;
			if(netepatilahetetaan[kohta]==0){
				if(player[i].epati[j].aika>0){
					netepatilahetetaan[kohta]=1;
					patimaara1++;
					patimaara++;
				}
			}else if(netepatilahetetaan[kohta]==2 || netepatilahetetaan[kohta]==1){
				if(player[i].epati[j].aika<=0){
					netepatilahetetaan[kohta]=3;
					patimaara++;
					patimaara2++;
				}
			}
		}
	}

	viesti[n]=(char)patimaara1;
	n++;
	maara=0;
	for(i=0;i<players;i++){
		for(j=0;j<player[i].alus.maxepadit;j++){
			int kohta=i*maksimiepadit+j;
			if(netepatilahetetaan[kohta]==1 && maara<patimaara1){
				netepatilahetetaan[kohta]=2;
				netpati.kulma=player[i].epati[j].kulma;
				netpati.x=player[i].epati[j].x;
				netpati.y=player[i].epati[j].y;
				netpati.xv=player[i].epati[j].xv;
				netpati.yv=player[i].epati[j].yv;
				netpati.r=player[i].epati[j].r;//*256.0f;
				netpati.g=player[i].epati[j].g;//*256.0f;
				netpati.b=player[i].epati[j].b;//*256.0f;
				netpati.pati=j;
				netpati.pelaaja=(char)i;
				//memcpy(&viesti[n],&netpati,sizeof(netPati));
				//n+=sizeof(netPati);
				int juttu=0;
				juttu=kirjotaNetpati(&viesti[n],&netpati);
				n+=juttu;
//				n+=kirjotaNetpati(&viesti[n],&netpati);
				maara++;
			}
		}
	}

	//laitetaan kuolleet epadit
	viesti[n]=(char)patimaara2;
	n++;
	maara=0;
	for(i=0;i<players;i++){
		for(j=0;j<player[i].alus.maxepadit;j++){
			int kohta=i*maksimiepadit+j;
			if(netepatilahetetaan[kohta]==3 && maara<patimaara2){
/*				netepatilahetetaan[kohta]=0;
				netpati.pati=j;
				netpati.pelaaja=(char)i;
				memcpy(&viesti[n],&netpati,sizeof(netPati));
				n+=sizeof(netPati);
				maara++;
*/
				netepatilahetetaan[kohta]=0;
				kuolevapati.pati=j;
				kuolevapati.pelaaja=(char)i;
				//memcpy(&viesti[n],&kuolevapati,sizeof(kuolevaPati));
				//n+=sizeof(kuolevaPati);
				int juttu=0;
				juttu=kirjotaKuolevapati(&viesti[n],&kuolevapati);
				n+=juttu;
				maara++;

			}
		}
	}


	/* pikkumiehia pikkumiehet ja v‰himm‰n merkitsev‰ss‰ bitiss‰ kerrotaan tuleeko limaa */
	maara=0;
	for(i=0;i<10;i++){
		if(siirrot[i].aika>0){
			maara++;
		}
	}
	maara=(maara << 1);
	if(limalaitetaan) maara++;
	viesti[n]=maara;
	maara=(maara >> 1);
	n++;
	int maara2=0;
	for(i=0;i<10;i++){
		if(siirrot[i].aika>0 && maara2<maara){
			maara2++;
			//memcpy(&viesti[n],&siirrot[i],sizeof(pikkumiessiirto));
			//n+=sizeof(pikkumiessiirto);
			n+=kirjotaPikkumiessiirto(&viesti[n],&siirrot[i]);
			siirrot[i].aika=0;
		}
	}

	//limat
	if(limalaitetaan){
		limalaitetaan=false;
		//viesti[n]=(char)1;
		//n++;
		//memcpy(&viesti[n],&netlima,sizeof(limajuttu));
		//n+=sizeof(limajuttu);
		n+=kirjotaLimajuttu(&viesti[n],&netlima);
	}else{
		//viesti[n]=(char)0;
		//n++;
	}

	/* end of pikkumiehet ja limat */

	if(gamemode==DOMINATION){
		viesti[n]=(char)dpisteita;
		n++;
		for(i=0;i<dpisteita;i++){
			viesti[n]=(char)dpiste[i].tiimi;
			n++;
		}
	}

	if(gamemode==CTF){
		char maara3=0;
		int ckohta=n;
		n++;
		for(i=0;i<cpisteita;i++){
			if(cpiste[i].enabled){
				maara3++;
				n+=kirjotaCPiste(&viesti[n],&cpiste[i]);
			}
		}
		viesti[ckohta]=maara3;
	}

	if(gamemode==MUSHROOM){
		unsigned char luku=0;
		for(i=0;i<spisteita;i++){
			if(spiste[i].uusiks){
				luku++;
			}
		}
		viesti[n]=luku;
		n++;
		if(luku>0){
			for(i=0;i<spisteita;i++){
				if(spiste[i].uusiks){
//					printf("l‰hetetty sieni\n");
					viesti[n]=(char)i;
					n++;
					n+=kirjotaSPiste(&viesti[n],&spiste[i]);
					spiste[i].uusiks=false;
				}
			}
		}
	}

	sprintf(&viesti[n],"plo;");
	n+=4;
	viesti[n]='\0';
	n++;

	int len=n;
	for(i=1;i<playerslots;i++)if(tila[i]>0){
		int result=SDLNet_TCP_Send(tcpsoketit[i], viesti,len);
		if(result<len) {
			printf("virhe l‰hetett‰ess‰ t‰yhk‰‰ sokettiin %i\n",i);
					int numused=SDLNet_TCP_DelSocket(hostset,tcpsoketit[i]);
					yhteyksia--;
			disconnect(i);
			printf("socket %i disconnected\n",i);
		}
	}
	
	/* eka viesti loppuu, toka alkaa */

	sprintf(viesti,"p2a");
	bool lahetetty[MAXNETPLAYERS];
	for(i=0;i<players;i++){
		lahetetty[i]=false;
	}
	for(i=0;i<players;i++){ //pelaajat jotka saavat valita uuden aluksensa
		if(playeruudestaan[i]==2 && mpstatus[i]!=2
			&& i>=hosteja && !lahetetty[pelaajasoketista[i]]){
			printf("l‰hetetty aluslaitto %i\n",pelaajasoketista[i]);
			n=3;
			lahetetty[pelaajasoketista[i]]=true;
			int temp=pelaajasoketista[i];
			int temp2=0;
			int kohta=n;
			n++;
			char maara=0;
			for(j=0;j<players;j++){
				if(pelaajasoketista[j]==temp){
					if(playeruudestaan[j]==2){
						playeruudestaan[j]=4;
						viesti[n]=(char)temp2;
						n++;
						viesti[n]=(char)j;
						n++;
						maara++;
					}
					temp2++;
				}
			}
			viesti[kohta]=(char)maara;
			sprintf(&viesti[n],"plo;");
			n+=8;
			len=n;
			int result=SDLNet_TCP_Send(tcpsoketit[pelaajasoketista[i]], viesti,len);
			if(result<len) {
				printf("virhe l‰hetett‰ess‰ pelinappeja %i\n",i);
					int numused=SDLNet_TCP_DelSocket(hostset,tcpsoketit[i]);
					yhteyksia--;
				disconnect(i);
				printf("socket %i disconnected\n",i);
			}
		}
	}


	//free(viesti);
	
}
bool asteroidiuudestaan[asteroideja];
char *netpatilahetetaan;
char *netepatilahetetaan;
bool limalaitetaan=false;

void lahetaNapitHostille(){
	int i;
	char viesti[14];
	sprintf(viesti,"nap");
	viesti[3]=(char)joinaavia;
	int n=4;
	for(i=0;i<joinaavia;i++){
		unsigned char napit=0;
		if(player[i].pylos)napit+=64;
		if(player[i].palas)napit+=2;
		if(player[i].pleft)napit+=4;
		if(player[i].pright)napit+=8;
		if(player[i].pfire)napit+=16;
		if(player[i].pease)napit+=32;
		viesti[n]=napit;
		n++;
	}
	viesti[n]=';';
	n++;
	viesti[n]='\0';
	n++;
//	viesti[joinaavia*sizeof(napit)+8]=';';
//	viesti[joinaavia*sizeof(napit)+9]='\0';

	//printf("l‰hetet‰‰n %i byte‰\n",n);	
	//int len=joinaavia*sizeof(napit)+10;
	int len=n;
	int result=SDLNet_TCP_Send(joinsoketti, viesti,len);
	if(result<len) {
		printf("virhe l‰hetett‰ess‰ nappeja, t‰ytyypi quitata kun ei muutakaan voi..\n");
		netGameQuit=true;
	}
	//free(viesti);
}


void joinKuuntelePeli(){
	int numready=0;
//	SDLNet_SocketSet set=SDLNet_AllocSocketSet(1);

//	int numused=SDLNet_TCP_AddSocket(set,joinsoketti);
	//printf("joinalku:%i\n",SDL_GetTicks());
	if(joinsoketti!=NULL) numready=SDLNet_CheckSockets(joinset, 0);
	if(numready==-1) {
		printf("SDLNet_CheckSockets: %s\n", SDLNet_GetError());
		perror("SDLNet_CheckSockets");
	}
	if(numready){
		//printf("joinalku:%i\n",SDL_GetTicks());
		if(SDLNet_SocketReady(joinsoketti)){
			int result;
			char msg[MAXLEN];
//			for(j=0;j<MAXLEN;j++) msg[j]=0;
			result=SDLNet_TCP_Recv(joinsoketti,msg,MAXLEN);
			if(result<0) {
				printf("jotain hassua p‰‰si k‰ym‰‰n, tuli %i byte‰, t‰ytyy pist‰‰ quittia..\n",result);// An error may have occured, but sometimes you can just ignore it
				netGameQuit=true;
				SDLNet_TCP_Close(joinsoketti);
				joinsoketti=NULL;
			}
			else if(result>0){
				kasittelePeliMsg(msg, result);
			}
		}
	}
	//printf("joinloppu:%i\n",SDL_GetTicks());
	//SDLNet_FreeSocketSet(set);





}


/*
	msg=palku (5)
	koordinaattien(pelaajien) m‰‰r‰ (1) (6)
	koordinaatteja (sizeof(coords)*%m‰‰r‰) (sizeof(coords)*%m‰‰r‰+6)
	uudestaantulevien m‰‰r‰ (1)
	alusten numeroita (n*2)

	*/
int packets=0,results=0;
char pelaajanumero[MAXNETPLAYERS];

void kasittelePeliMsg2(char *msg, int result){
	int n=3,i;
	char maara=msg[n];
	n++;
	for(i=0;i<maara;i++){
		char pelaaja=msg[n];
		n++;
		playeruudestaan[pelaaja]=2;
		char pelaaja2=msg[n];
		pelaajanumero[pelaaja]=pelaaja2;
		n++;
	}

	printf("pit‰is alus valita..\n");
	if(result>n+10){
		for(i=0;i<10;i++){
			if(msg[n]==';'){
				n++;
				if(msg[n]!='p')n++;
				if(msg[n]!='p')n++;
				if(msg[n]!='p')n++;
				if(result-n>7){
					packets--;
					results-=result-n;
					kasittelePeliMsg(&msg[n], result-n);
				}
			}
			n++;
		}
	}


}

char voittotiimi=0;
void kasitteleVictoryMsg(char *msg, int result){
	int n=7,i;
	if(msg[n]=='o'){
		n++;
		voittotiimi=msg[n];
		gamemode=DOMINATION;
	}else if(msg[n]=='d'){
		gamemode=DEATHMATCH;
		n++;
		voittotiimi=msg[n];
		n++;
		char maara=msg[n];
		n++;
		for(i=0;i<maara;i++){
			VictoryPlayer vp;
			memcpy(&vp,&msg[n],sizeof(VictoryPlayer));
			int numero=vp.numero;
			int pisteet=vp.pisteet;
			player[numero].pisteet=vp.pisteet;
			n+=sizeof(VictoryPlayer);
		}
	}
	netGameQuit=true;
	SDLNet_TCP_Close(joinsoketti);
	joinsoketti=NULL;
	netVictoryLoop();
	netGameQuit=true;
}

void kasittelePeliMsg(char *msg, int result){
	//printf("result=%i\n",result);
	int i;
	packets++;
	results+=result;
	if(compare(msg,"p2a",2)){
		kasittelePeliMsg2(msg, result);
		return;
	}
	else if(compare(msg,"victory",6)){
		kasitteleVictoryMsg(msg,result);
		return;
	}else if(compare(msg,"tiimit",5)){
		joinwaitKasittele(msg,result);
		return;
	}else if(compare(msg,"pelaajia montako",5)){
		joinwaitKasittele(msg,result);
		return;
	}else if(compare(msg,"game started",6)){
		joinwaitKasittele(msg,result);
		return;
	}else if(compare(msg,"kaikki kunnossa",6)){
		joinwaitKasittele(msg,result);
		return;
	}else if(compare(msg, "x‰t",3)){
		joinwaitKasittele(msg,result);
	}else if(compare(msg,"quiting",6)){
		netGameQuit=true;
		return;
	}
	if(!compare(msg,"pal",2)) return;
	int maara=msg[3];
	int n=4;
//	coords koordinaatit;

	for(i=0;i<maara;i++) {
		if(result>(n+coordkoko)){
			n+=lueCoords(&player[i],&msg[n]);
		}else return;
	}
	//memcpy(&netframe,&msg[n],2);
	//n+=2;
	//netframe=msg[n];
	//printf("netframe=%i\n",netframe);
	netframe++;

		
	//asteroidit
	if(result>n) maara=msg[n];
	else return;
	n++;
	int luku=0;
	for(i=0;i<maara;i++){
		if(result>(n+2+asteroidikoko)){
			char luku5=msg[n];
			n++;
			//memcpy(&asteroidi[luku5],&msg[n],sizeof(Pati));
			//n+=luePati(&asteroidi[luku5],&msg[n]);
			n+=lueAsteroidi(&asteroidi[luku5],&msg[n]);
			//n+=sizeof(Pati);
			if(n>result){
				printf("jotain m‰tt‰‰ i=%i, n=%i\n",i,n);
				i=maara;
			}
		}else return;
	}
	char luku3;
	if(result>n) luku3=msg[n];
	else return;
	n++;
	if(luku3&1){
		//luku3--;
		if(result>(n+1+asteroidipoxkoko)){
			//memcpy(&arajahdys,&msg[n],sizeof(asteroidipox));
			//n+=sizeof(asteroidipox);
			n+=lueAsteroidipox(&arajahdys,&msg[n]);
			asteroidirajahtaa=true;
		}else return;
	}


	//r‰j‰ht‰v‰t playerit
	//if(result>n) maara=msg[n];
	//n++;
	maara=(luku3>>2);
	for(i=0;i<maara;i++){
		if(result>(n+1+asteroidipoxkoko)){
			luku=msg[n];
			n++;
			if(luku<players){
				playerrajahtaa[luku]=true;
				//memcpy(&prajahdys[luku],&msg[n],sizeof(asteroidipox));
				//n+=sizeof(asteroidipox);
				n+=lueAsteroidipox(&prajahdys[luku],&msg[n]);
				printf("r‰j‰ytet‰‰n %i\n",luku);
			}
		}
	}

	if(luku3&2){
		if(result>n) maara=msg[n];//pelaajat jotka laitetaan uudestaan..
		else return;
		n++;
		for(i=0;i<maara;i++){
			if(result>n+2){
				char ms1=msg[n];
				char ms2=msg[n+1];
				printf("player[%i].uudestaan(alus[%i])\n",msg[n],msg[n+1]);
				player[msg[n]].uudestaan(alus[msg[n+1]]);
				player[msg[n]].laitaAlkuSirpaleet();	

				n+=2;
			}else return;
		}
	}


	//uusia pateja
	//n++;
	netPati netpati;
	char luku2;
	if(result>n) luku2=msg[n];
	maara=luku2;
//	printf("%i kutia tuli %i\n",msg[n]);
	n++;
	for(i=0;i<maara;i++){
		if(result>(n+10)){
			//printf("saatu %i patia\n",maara);
			//memcpy(&netpati,&msg[n],sizeof(netPati));
			//n+=sizeof(netPati);
			int juttu=0;
			juttu=lueNetpati(&netpati,&msg[n]);
			n+=juttu;
			char pelaaja=netpati.pelaaja;
			int pati=netpati.pati;
			if(pelaaja<players && pati<maksimipadit && pelaaja>=0 && pati>=0){
				player[pelaaja].pati[pati].kulma=netpati.kulma;
				player[pelaaja].pati[pati].x=netpati.x;
				player[pelaaja].pati[pati].y=netpati.y;
				player[pelaaja].pati[pati].xv=netpati.xv;
				player[pelaaja].pati[pati].yv=netpati.yv;
				player[pelaaja].pati[pati].r=netpati.r;///256.0f;
				player[pelaaja].pati[pati].g=netpati.g;///256.0f;
				player[pelaaja].pati[pati].b=netpati.b;///256.0f;
				player[pelaaja].pati[pati].aika=player[pelaaja].alus.patiaika;
				player[pelaaja].pati[pati].energia=player[pelaaja].alus.patienergia;
				player[pelaaja].pati[pati].koko=player[pelaaja].alus.patisade;
				if(player[pelaaja].alus.numero==13){
					//printf("imurialus pati %i luotu,r=%f,g=%f\n",pati,player[pelaaja].pati[pati].r,player[pelaaja].pati[pati].g);
				}
			}else printf("yritettiin laittaa pelaajalle %i patia %i, yhteens‰ tuli %i",pelaaja,pati,maara);
		}else return;
	}

	//kuolevia pateja
	kuolevaPati kuolevapati;
	if(result>=n)luku2=msg[n];
	else return;

	maara=luku2;

	n++;
//	printf("eka %i,toka %i\n",msg[n-1],msg[n]);
//	int j;
	for(i=0;i<maara;i++){
/*		if(result>=n+sizeof(netPati)){
			//if(i==0) printf("saatu %i kuolevaa patia\n",maara);
			memcpy(&netpati,&msg[n],sizeof(netPati));
			int pelaaja=netpati.pelaaja;
			int pati=netpati.pati;
			if(pelaaja<players && pati<maksimipadit && pelaaja>=0 && pati>=0){
				memset(&player[pelaaja].pati[pati],0,sizeof(Pati));
			}else printf("yritettiin tuhota pelaajalta %i patia %i, yhteens‰ tuli %i\n",pelaaja,pati,maara);
			n+=sizeof(netPati);
		}else return;*/


		if(result>=(n+kuolevapatikoko)){
			//if(i==0) printf("saatu %i kuolevaa patia\n",maara);
			//memcpy(&kuolevapati,&msg[n],sizeof(kuolevaPati));
			//n+=sizeof(kuolevaPati);
			n+=lueKuolevapati(&kuolevapati,&msg[n]);
			int pelaaja=kuolevapati.pelaaja;
			int pati=kuolevapati.pati;
			if(pelaaja<players && pati<maksimipadit && pelaaja>=0 && pati>=0){
				memset(&player[pelaaja].pati[pati],0,sizeof(Pati));
			}else printf("yritettiin tuhota pelaajalta %i patia %i, yhteens‰ tuli %i\n",pelaaja,pati,maara);
		}else return;

	}


	//uusia epateja
	if(result>n) luku2=msg[n];
	else return;
	maara=luku2;
	n++;
	//printf("maara=%i\n",maara);
	for(i=0;i<maara;i++){
		if(result>=(n+netpatikoko)){
			//if(i==0)printf("saatu %i epatia\n",maara);
			n+=lueNetpati(&netpati,&msg[n]);
			//memcpy(&netpati,&msg[n],sizeof(netPati));
			//n+=sizeof(netPati);
			char pelaaja=netpati.pelaaja;
			int pati=netpati.pati;
			if(pelaaja<players && pati<maksimiepadit && pelaaja>=0 && pati>=0){
				//printf("xv=%f,yv=%f",netpati.xv,netpati.yv);
				player[pelaaja].epati[pati].kulma=netpati.kulma;
				player[pelaaja].epati[pati].x=netpati.x;
				player[pelaaja].epati[pati].y=netpati.y;
				player[pelaaja].epati[pati].xv=netpati.xv;
				player[pelaaja].epati[pati].yv=netpati.yv;
				player[pelaaja].epati[pati].r=netpati.r;///256.0f;
				player[pelaaja].epati[pati].g=netpati.g;///256.0f;
				player[pelaaja].epati[pati].b=netpati.b;///256.0f;
				player[pelaaja].epati[pati].aika=player[pelaaja].alus.easekesto;
				player[pelaaja].epati[pati].energia=player[pelaaja].alus.epatienergia;
				player[pelaaja].epati[pati].koko=player[pelaaja].alus.epatisade;
				if(player[pelaaja].alus.numero==5){
					//printf("ukkeli %i aika=%i\n",pati,(int)player[pelaaja].epati[pati].aika);
				}
			}
		}else return;
	}

	//kuolevia epateja
	if(result>n) luku2=msg[n];
	maara=luku2;
	n++;
	for(i=0;i<maara;i++){
/*		if(result>=n+sizeof(netPati)){
			memcpy(&netpati,&msg[n],sizeof(netPati));
			int pelaaja=netpati.pelaaja;
			int pati=netpati.pati;
			if(pelaaja<players && pati<maksimipadit && pelaaja>=0 && pati>=0){
				memset(&player[pelaaja].epati[pati],0,sizeof(Pati));
			}
			n+=sizeof(netPati);
		}else return;
*/

		if(result>=(n+kuolevapatikoko)){
			n+=lueKuolevapati(&kuolevapati,&msg[n]);
			//memcpy(&kuolevapati,&msg[n],sizeof(kuolevaPati));
			int pelaaja=kuolevapati.pelaaja;
			int pati=kuolevapati.pati;
			if(pelaaja<players && pati<maksimipadit && pelaaja>=0 && pati>=0){
				memset(&player[pelaaja].epati[pati],0,sizeof(Pati));
			}
			//n+=sizeof(kuolevaPati);
		}else return;

	}

/* Pikkumiehet ja limat */
	//pikkumiehia
	luku=0;
	if(result>n) luku3=msg[n];
	else return;
	n++;
	if(luku3&1) luku2=1;
	else luku2=0;

	luku3=(luku3 >> 1);
	for(i=0;i<luku3;i++) if(result>(n+16)) {
		//memcpy(&siirrot[0],&msg[n],sizeof(pikkumiessiirto));
		//n+=sizeof(pikkumiessiirto);
		n+=luePikkumiessiirto(&siirrot[0],&msg[n]);
		int numero=siirrot[0].numero;
		//printf("ctf:%i,%i,%f,%f,%f\n", siirrot[0].aika,siirrot[0].numero,
		//	siirrot[0].x,siirrot[0].y,siirrot[0].energia);
		pikkumies[numero].aika=siirrot[0].aika;
		pikkumies[numero].energia=siirrot[0].energia;
		pikkumies[numero].x=siirrot[0].x;
		pikkumies[numero].y=siirrot[0].y;
		pikkumies[numero].ampumisviive=70;
		pikkumies[numero].xv=0;
		pikkumies[numero].yv=0;

	}

	//limoja
	if(luku2>0){
		limajuttu templima;
		//memcpy(&templima,&msg[n],sizeof(limajuttu));
		//n+=sizeof(limajuttu);
		n+=lueLimajuttu(&templima,&msg[n]);
		if(templima.player>=0 && templima.player<players && templima.kohta>=0 &&templima.kohta<maxlimat){
			player[templima.player].lima[templima.kohta].x=templima.x;
			player[templima.player].lima[templima.kohta].y=templima.y;
			player[templima.player].lima[templima.kohta].aika=400;
		}
	}

	/* end of pikkumiehet ja limat */

	if(gamemode==DOMINATION){
		char maara;
		if(result>n) maara=msg[n];
		n++;
		for(i=0;i<maara;i++) if(result>n) {
			char luku=msg[n];
			dpiste[i].tiimi=luku;
			n++;
		}
	}

	if(gamemode==CTF){
		char maara3=0;
		if(result>n) maara3=msg[n];
		n++;
		for(i=0;i<maara3;i++) if(result>n+cpistekoko) {
			CPiste tempcpiste;
			//memcpy(&tempcpiste,&msg[n],sizeof(CPiste));
			n+=lueCPiste(&tempcpiste,&msg[n]);
			char luku=tempcpiste.numero;
			memcpy(&cpiste[luku],&tempcpiste,sizeof(CPiste));
/*			cpiste[luku].enabled=tempcpiste.enabled;
			cpiste[luku].x=tempcpiste.x;
			cpiste[luku].y=tempcpiste.y;
			cpiste[luku].player=tempcpiste.player;*/
			//n+=sizeof(CPiste);
		}
	}

	if(gamemode==MUSHROOM){
		unsigned char luku=msg[n];
		n++;
		for(i=0;i<luku;i++) if(result>n+spistekoko) {
			unsigned char luku2=msg[n];
			n++;
			n+=lueSPiste(&spiste[luku2],&msg[n]);
			printf("vastaanotettu sieni:l=%i,x=%f,y=%f\n",luku2,spiste[luku2].x, spiste[luku2].y);
		}
	}

	if(result>n+7){
		//printf("Messagea j‰i j‰lelle %i byte‰ -> msg=%s\n",result-n,&msg[n]);
		for(i=0;i<7;i++){
			if(msg[n]==';'){
				n++;
				if(msg[n]!='p')n++;
				if(msg[n]!='p')n++;
				if(msg[n]!='p')n++;
				//printf("jatketaan viestin k‰sittely‰ kohdasta %s\n",&msg[n]);
				if(result-n>3){
					//packets--;
					results-=result-n;
					kasittelePeliMsg(&msg[n], result-n);
				}
			}
			n++;
		}
	}


}
limajuttu netlima;
