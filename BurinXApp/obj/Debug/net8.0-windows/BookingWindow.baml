   M S B A M L   `   `   `  ����  	BurinXApp    BurinXApp.BookingWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Icon$  /Sources/Icon.png��5   	    � Title$ BurinX��5   	   $	��450��6   $	��600��6%   ���#1F1F1F � WindowStartupLocation$ CenterScreen=�6F    � 	Resources 5         AccentBrush �� 
StartPoint �� EndPoint � Color � Offset��%�   &	        �� 5   
   $	 0,0&�62   $	 1,1&�6C   .��5      � $ #FF00FF��6   $ 0Q�6+   � 5	      $ #00FFFF��6   $ 1Q�6+   5
      5      .��5      � .+�5   
   �� $X��RВыберите специальный пакет игрового времени��6   $��Center=�6G   $	��Top=�6d   $��24(�5      $
��Bold"�6"   ���#FFFFFF$��0,10,0,0q�6I   �� 5   
    �� Orientation$ 
Horizontal=�6   $��Center=�6.   $	��Top=�6K   $��	10,60,0,0q�6c   .+�5      �� $��Ваш баланс:��6   ���#CCCCCC$��16(�6@   �� 5      -   5        	 ��Name$	 BalanceTextBlock��5      $��$0.00��62   ���#83B8FF$��16(�6T   5      �� 5   
   -   5        
 ��Name$
 PackageComboBox��5      $��Center=�6,   $	��Top=�6I   $��
10,100,0,0q�6a   $	��360��6u   $��16(�6�   5      �� 5   
   $ 
Horizontal=�6   $��Center=�6.   $	��Top=�6K   $��
10,160,0,0q�6c   .+�5      �� $>��8Укажите колмчество часов игры:��6   ���#CCCCCC$��16(�6S   $��Center=�6a   �� 5      -   5         ��Name$ HoursTextBox��5      $��50��6,   $��16(�67   $��Center=�6l   $
��10,0q�6�   5      �� 5"   
   -   5        $-��'Забронировать сейчас��5"      $	��180��60   $��40��6<   #��[  6H   
�������$��16(�6�   $��Center=�5#      $��Bottom=�6.   $��0,0,0,30q�6I   6\   �� 5&   
   -   5        $	 PackageInfoTextBlock��5&      $��Center=�62   $	��Top=�6O   $��
10,220,0,0q�5'      $��16(�6(   ���#CCCCCC$	��550��6K   �� 5(   
   -   5        $	 TotalCostTextBlock��5(      $��Center=�60   $	��Top=�6M   $��
10,240,0,0q�5)      $��16(�6(   ���#83B8FF$	��550��6K   5*      5+      