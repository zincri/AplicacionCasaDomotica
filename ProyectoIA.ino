#include <SoftwareSerial.h>
char estado = 'Z'; 
int led1=2;
int led2=3;
int led3=4;
int led4=5;
int led5=6;
int led6=7;
int pulsador = 9;
int buzzer  = 8;
void setup()  
{
  pinMode(led1, OUTPUT);
  pinMode(led2, OUTPUT);
  pinMode(led3, OUTPUT);
  pinMode(led4, OUTPUT);
  pinMode(led5, OUTPUT);
  pinMode(led6, OUTPUT);


  pinMode(pulsador, INPUT_PULLUP);
  pinMode(buzzer, OUTPUT);
  
  Serial.begin(9600);
}
    
char led; // Led es nuestro led conectado a Arduino
void loop() 
{
  if(Serial.available()>0){        // lee el bluetooth y almacena en estado
    
    estado = Serial.read();  
    
    Serial.println(estado);
    
  }
    if (estado=='A')
    {
      digitalWrite(led1, HIGH);
    }
    if (estado=='B')
    {
      digitalWrite(led1, LOW);
    }  
    if (estado=='C')
    {
      digitalWrite(led2, HIGH);
    }
    if (estado=='D')
    {
      digitalWrite(led2, LOW);
    }  
    if (estado=='E')
    {
      digitalWrite(led3, HIGH);
    }
    if (estado=='F')
    {
      digitalWrite(led3, LOW);
    }  
    if (estado=='P')
    {
      digitalWrite(led4, HIGH);
    }
    if (estado=='Q')
    {
      digitalWrite(led4, LOW);
    }
    //...ventilador
     if (estado=='G')
    {
      analogWrite(led5, 255);
      analogWrite(led6,0);
    }
    if (estado=='H')
    {
      analogWrite(led5, 0);
      analogWrite(led6, 0);
      
    }   
   //..
   if (estado=='N')
    {
      //for(int i = 0; i<5;i++){
        digitalWrite(buzzer,HIGH);
        delay(500);
        //digitalWrite(buzzer,LOW);
        //delay(500);
        //}
    }
    if (estado=='O')
    {
      digitalWrite(buzzer,LOW);
      
    } 
    if(digitalRead(pulsador) == LOW){
      //Serial.println("PULSADO");
      Serial.write('X');
      estado='N';
      //for(int i = 0; i<7;i++){
        digitalWrite(buzzer,HIGH);
        delay(500);
        //digitalWrite(buzzer,LOW);
        //delay(500);
       // }
      
      delay(500);
    }
   
}
