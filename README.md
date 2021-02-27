﻿# mucomMD2vgm
メガドライブ向けVGM/XGMファイル作成ツール  
  
[概要]  
 このツールは、ユーザーが作成したmucom形式のMMLファイルを元にVGMファイル又はXGMファイルを作成します。  
  
[機能、特徴]  
 ・メガドライブの音源構成(YM2612 + SN76489)にそったVGM/XGMを生成します。  
  又は、OPL,OPMを使用したVGMを生成します。  
 ・FM音源(YM2612)は最大6ch使用可能です。  
 効果音モードを使用すると+3ch使用可能です。  
 ・PCM(YM2612)を1ch使用可能です。(FM音源1chと排他的に使用します。)  
 XGMの場合はPCM(YM2612)を4ch使用可能です。(FM音源1chと排他的に使用します。)  
 ・PSG(DCSG)音源(SN76489)は4ch(ノイズチャンネルを除く)使用可能です。  
 ・以上、メガドライブ音源系だけで最大14ch(XGMは17ch)使用可能です。  
 OPLは14ch使用可能です。  
 OPMは8ch使用可能です。  
 ・MMLの仕様はmucom88に準拠します。  
  
[必要な環境]  
 ・Windows7以降のOSがインストールされたPC  
 ・テキストエディタ  
 ・VGMを演奏するプレイヤアプリ(MDPlayerを推奨)  
 ・気合と根性  
  
[著作権・免責]  
mucomMD2vgm,mdvcはMITライセンスに準ずる物とします。LICENSE.txtを参照。  
著作権は作者が保有しています。  
このソフトは無保証であり、このソフトを使用した事による  
いかなる損害も作者は一切の責任を負いません。  
また、MITライセンスは著作権表示および本許諾表示を求めますが本ソフトでは不要です。  
  
以下のソフトウェアのソースコードをC#向けに改変し使用しています。  
又はコードを提供していただいております。  
これらのソースは各著作者が著作権を持ちます。  
ライセンスに関しては、各ドキュメントを参照してください。  
  
該当ソース：  
  Common.cs中の最大公約数、最小公倍数算出ルーチン(MIT)  
    作者：くろま さん  
  
  
[SpecialThanks]  
 本ツールは以下の方々にお世話になっております。また以下のソフトウェア、ウェブページを参考、使用しています。  
 本当にありがとうございます。  
  
 ・WING☆ さん  
 ・くろま さん  
 ・欧場豪@マシㇼキ提督さん  

 ・mucom88/mucom88win  
 ・Music LALF  
 ・Visual Studio Community 2017  
 ・VGM Player  
 ・さくらエディター  
  
  
