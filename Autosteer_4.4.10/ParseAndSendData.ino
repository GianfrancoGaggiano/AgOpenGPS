void ParseData(byte* Data, int len)
{
  if (Data[0] == 0x00 && Data[1] == 0x00) return;
  if (Data[0] == 0x7F && Data[1] == 0x70) //Send_AutoSteer
  {
    float gpsSpeed = ((float)(Data[3] << 8 | Data[4])) * 0.025;

    //distance from the guidance line in mm
    float distanceFromLine = (float)(Data[5] << 8 | Data[6]);   //high,low bytes

    //set point steer angle * 100 is sent
    steerAngleSetPoint = ((float)(Data[7] << 8 | Data[8])); //high low bytes
    steerAngleSetPoint *= 0.01;

    if (!distanceFromLine == 32030 && (distanceFromLine == 32020 || distanceFromLine == 32000 
        || gpsSpeed < aogSettings.minSteerSpeed || gpsSpeed > aogSettings.maxSteerSpeed || steerSwitch == 1 ))
    {
         watchdogTimer = 12;//turn off steering motor
    }
    else watchdogTimer = 0;  //reset watchdog


    if (aogSettings.BNOInstalled || aogSettings.InclinometerInstalled)
    {
      toSend[1] = 0xC2;
      toSend[2] = 0x07;
      if (aogSettings.BNOInstalled)
      {
        int temp = IMU.euler.head;
        toSend[3] = (byte)(temp >> 8);
        toSend[4] = (byte)(temp);
      }
      else//9999
      {
        toSend[3] = 0x27;
        toSend[4] = 0x0F;
      }
      if (aogSettings.InclinometerInstalled)
      {
        int temp = (int)XeRoll;
        toSend[5] = (byte)(temp >> 8);
        toSend[6] = (byte)(temp);
      }
      else//9999
      {
        toSend[5] = 0x27;
        toSend[6] = 0x0F;
      }
      SendData(toSend, 7);
    }
    
    
    // Send Data to AOG
    toSend[1] = 0xC0;
    toSend[2] = 0x06;
    

    //actual steer angle
    int temp = (100 * steerAngleActual);
    toSend[3] = (byte)(temp >> 8);
    toSend[4] = (byte)(temp);

    //pwm value
    if (pwmDisplay < 0) pwmDisplay*= -1;
    toSend[5] = pwmDisplay;

    //off to AOG
    SendData(toSend, 6);
    
  }
#if (Enable_Section_Control_Output)
  else if (Data[0] == 0x7F && Data[1] == 0x71) //Send_Sections
  {
    int count = ((Data[2]-4) * 8) - 8 + Data[3];
    
    int i = 0;
    int bits = 0;
    
    for (int k = 0; k < count && k < sizeof(Sections_Output); k++)
    {
      byte ff = Data[4 + i];
      if ((ff & (1 << bits)) == (1 << bits))
      {
        if (Sections_Output[k]) digitalWrite(Sections_Output[k],HIGH)
      }
      else if (Sections_Output[k]) digitalWrite(Sections_Output[k],LOW)

      bits++;
      if (bits == 8)
      {
        bits = 0;
        i++;
      }
    }
  }
#endif
  else if (Data[0] == 0x7F && Data[1] == 0x72) //Send_AutoSteerButton
  {
    pulseCount=0;
    if (aogSettings.SteerSwitch == 0) //steer button
    {
      if (Data[3] == 0x01) steerSwitch = 1;
      else steerSwitch = 0;
    }
  }
#if (Enable_Hydraulic_Lift)
  else if (Data[0] == 0x7F && Data[1] == 0x73) //Send_HydraulicLift
  {
    HydrLiftWatchdog = 0;
    if (LastTimer != Data[3])
    {
      if (Data[3] == 0x02)//Up
      {
        RaiseTimer = HydraulicLift.raiseTime * 2;
        if (aogConfig.isRelayActiveHigh) digitalWrite(RAISE,LOW);
        else digitalWrite(RAISE,HIGH);
        LowerTimer = 1;
      }
      else if (Data[3] == 0x01)//Down
      {
        LowerTimer = HydraulicLift.lowerTime * 2;
        if (aogConfig.isRelayActiveHigh) digitalWrite(LOWER,LOW);
        else digitalWrite(LOWER,HIGH);
        RaiseTimer = 1;
      }
      else//Off
      {
        LowerTimer = 1;
        RaiseTimer = 1;
      }
    }
  }
#endif
  else if (Data[0] == 0x7F && Data[1] == 0x74) //Send_Uturn
  {
  }
  else if (Data[0] == 0x7F && Data[1] == 0x75) //Send_Treeplant
  {
  }
  else if (Data[0] == 0x7F && Data[1] == 0xF8) //Machine Data Config
  {
    HydraulicLift.RaiseTime = Data[3];
    HydraulicLift.LowerTime = Data[4];
    HydraulicLift.Enabled = Data[5];
    if (bitRead(Data[6],0)) HydraulicLift.IsRelayActiveHigh = 1; else HydraulicLift.IsRelayActiveHigh = 0;
    
    //save in EEPROM and restart
    EEPROM.put(60, HydraulicLift);
    //resetFunc();
  }
  else if (Data[0] == 0x7F && Data[1] == 0xFB) //Arduino Steer Config
  {
    if (bitRead(Data[3],0)) aogSettings.InvertWAS = 1; else aogSettings.InvertWAS = 0;
    if (bitRead(Data[3],1)) aogSettings.InvertRoll = 1; else aogSettings.InvertRoll = 0;
    if (bitRead(Data[3],2)) aogSettings.MotorDriveDirection = 1; else aogSettings.MotorDriveDirection = 0;
    if (bitRead(Data[3],3)) aogSettings.SingleInputWAS = 1; else aogSettings.SingleInputWAS = 0;
    if (bitRead(Data[3],4)) aogSettings.CytronDriver = 1; else aogSettings.CytronDriver = 0;
    if (bitRead(Data[3],5)) aogSettings.SteerSwitch = 1; else aogSettings.SteerSwitch = 0;
    if (bitRead(Data[3],6)) aogSettings.UseMMA_X_Axis = 1; else aogSettings.UseMMA_X_Axis = 0;
    if (bitRead(Data[3],7)) aogSettings.ShaftEncoder = 1; else aogSettings.ShaftEncoder = 0;

    //set1
     if (bitRead(Data[4],0)) aogSettings.BNOInstalled = 1; else aogSettings.BNOInstalled = 0;
     if (bitRead(Data[4],1)) aogSettings.isRelayActiveHigh = 1; else aogSettings.isRelayActiveHigh = 0;
     if (bitRead(Data[4],2)) aogSettings.WorkSwActiveLow = 1; else aogSettings.WorkSwActiveLow = 0;
     if (bitRead(Data[4],3)) aogSettings.WorkSwManual = 1; else aogSettings.WorkSwManual = 0;
     
    aogSettings.maxSteerSpeed = Data[5]; //actual speed * 5
    aogSettings.minSteerSpeed = Data[6];

    aogSettings.InclinometerInstalled = Data[7] & 192;
    aogSettings.InclinometerInstalled = aogSettings.InclinometerInstalled >> 7;
    aogSettings.PulseCountMax = Data[7] & 63;

    aogSettings.AckermanFix = Data[7];

    byte checksum = 0;
    for (int i = 3; i < Data[2]; i++) checksum += Data[i];

    //send Data back - version number. 
    SendTwoThirty((byte)checksum);
      
    EEPROM.put(40, aogSettings);

    //reset Arduino
    resetFunc();
  }
  else if (Data[0] == 0x7F && Data[1] == 0xFC)//AutoSteer settings
  {
    steerSettings.Kp = (float)Data[3];       // read Kp from AgOpenGPS
    steerSettings.lowPWM = (float)Data[4];     // read lowPWM from AgOpenGPS
    steerSettings.Kd = (float)Data[5] * 1.0;       // read Kd from AgOpenGPS
    steerSettings.Ko = (float)Data[6] * 0.1;       // read Ko from AgOpenGPS
    steerSettings.steeringPositionZero = (WAS_ZERO) + Data[7];//read steering zero offset
    
    steerSettings.minPWM = Data[8]; //read the minimum amount of PWM for instant on
    steerSettings.highPWM = Data[9]; //
    steerSettings.steerSensorCounts = Data[10]; 

    byte checksum = 0;
    for (int i = 3; i < Data[2]; i++) checksum += Data[i];
      
    //send Data back - version number. 
    SendTwoThirty((byte)checksum);
    
    EEPROM.put(10, steerSettings);

    // for PWM High to Low interpolator
    
    highLowPerDeg = (steerSettings.highPWM - steerSettings.lowPWM) / LOW_HIGH_DEGREES;
    //resetFunc();
  }
}

//send back checksum and version
void SendTwoThirty(byte check)
{
    toSend[1] = 0xC6;
    toSend[2] = 0x05;
    toSend[3] = check;
    toSend[4] = aogVersion;
    
    SendData(toSend, 5);
}

void SendData(byte* Data, int len)
{
  #if (Enable_UDP)
    ether.sendUdp(Data, len, portMy, ipDestination, portDestination);
  #else
    Serial.write(Data, len);
    Serial.flush();
  #endif
}
