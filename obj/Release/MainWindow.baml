   M S B A M L   `   `   `  ���� G  CExportWadLol, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:ExportWadLolExportWadLol      ExportWadLol.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     ,localclr-namespace:ExportWadLol       � 
ResizeMode$  NoResize=� � Title$ ExLOLPatch - Giấy Nháp��$	��386��$	��300��.���  � ColumnDefinitions ��  � RowDefinitions �� $
��132* ��� $
��287* �.+��� .+��� $
��40px(�.��
ExLOLPatch�� .��Ver 1.0 - Giấy nháp�� $
��10px(�.��97Extract wad, wad.client files of Legend of Lengend game�� $��1��.+�� -   $��progress��$
��10px������  �� 
TargetType" �.���  �� Property � �� �� Value ��  �� 
TargetType" �.R��� $��0q����#333 �� CornerRadius$ 0��	 �� Padding$	 0q�._�� 
 �Name$
 
PART_Track��.+���  ��Name$ PART_Indicator��$
��Left=����#6d9aed�� -   $��	TractName�����#333333���#EEE$
��10px(�$
��25px���� -   $
��btn1�� J� 	AllowDrop . $	��211��.���� $��Center=�$��Center=����#EEE���#222222$
��50pxq�
���   �$��1q�.��Drag and Drop file(s) here���� " ��.���  � �� �#333333��  � ��$ 0q���  � �� �� " ��.R��� #��z��
������ $��0q�._��� $��Center=�$��Center=� �� Triggers P�  P� Property �@J�IsMouseOver P� Value . . ���  � �� �#888888