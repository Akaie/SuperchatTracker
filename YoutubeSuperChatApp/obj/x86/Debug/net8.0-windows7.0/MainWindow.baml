   M S B A M L   `   `   `  ����T  PYouTube SuperChat Tracker, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:YoutubeSuperChatAppYoutubeSuperChatApp  5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089>.clr-namespace:System.Xaml;assembly=System.XamlSystem.Xaml 5   	   $    YoutubeSuperChatApp.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   K GUno.Xaml, Version=255.255.255.255, Culture=neutral, PublicKeyToken=null<x,http://schemas.microsoft.com/winfx/2006/xaml      5   	   Bd2http://schemas.microsoft.com/expression/blend/2008      5   	   Lmc;http://schemas.openxmlformats.org/markup-compatibility/2006      5   	   5local!clr-namespace:YoutubeSuperChatApp      5   	   >s.clr-namespace:System.Xaml;assembly=System.Xaml      5   	     � Title$  YouTube SuperChat Tracker��5	   	   $	��420��6+   $	��800��68   $	��800��6D   $	��800��6S   $	��460��6b   $	��460��6r   $��Disabled=�6�    � 
ResizeMode$ NoResize=�6�   .��5
      � $	��404��6    � ColumnDefinitions 5   
   �� 5      6!    � RowDefinitions 5   
   �� 5      $��35 �6   6*   �� 5      6   �� 5      $��65 �6   6*   .+�5   
   o� -   5         o�Name$
 menu��5      $��0��6   $��0��6*   $	��Top=�6:   $��20               ��6R   .@�5      m� -   5        $��Options��5      $��14(�6@   m� 5      -   5        $��View Resolved��5      $��14(�6K   m� 5      -   5        $
��Exit��5      $��14(�6:   5      �� 5   
   -   5         ��Name$ 
recordList��5      $��1��6'   $��0��64   $��Center=�6D   $	��300��6_   $��8,0,8,0q�6l   $��14(�6}   ��6�   ��  �� Path$ 	ListItems���5      �� 5       �� DataType$ local:ListViewItem��6   .R�5      �� .+�5      � .+�5      �� $��	10 0 10 0q�6c   ��6(   �� $
 Name���6C   �� $	 Clr�6c   �� 5      $��	10 0 10 0q�6e   ��6(   �� $ Amount���6E   �� $	 Clr�6e   �� 5       $��	10 0 10 0q�6c   ��6(   �� $
 Date���6C   �� $	 Clr�6c   5!      5"      5#      5%      � 5&   
   $��2��6   $��0��6   $��Center=�6,   $��60��6G    5'      �� 5(      6%   �� 5)      6%    5+      �� 5,      $��30 �6    6.   �� 5-      $��30 �6    6.   .+�5/      �� -   5         ��Name$ StartBtn��5/      $��Start Polling��6'   $	��250��6?   $��25��6K   $��0��6W   $��0��6d   $��14(�6�   �� 50      -   5        $ StopBtn��50      $��Stop Polling��6&   $	��250��6=   $��25��6I   $��0��6U   $��1��6e   $��14(�6�   �� 51      -   5        $ 	RemoveBtn��51      $��Resolve��6(   $	��250��6:   $��25��6F   $��0��6R   $��1��6_   $��14(�6o   6}   �� 52      -	   5        $ AllBtn��52      $��Resolve All��6%   $	��250��6;   $��25��6G   $��1��6S   $��1��6`   $��14(�6p   6~   53      55      56      