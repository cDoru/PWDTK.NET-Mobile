using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using PWDTK_MOBILE_WP_8;

namespace PWDTK_Mobile_Demo_GUI
{
    public partial class MainPage : PhoneApplicationPage
    {
        Byte[] salt;
        Byte[] hash;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
           salt = PWDTK.GetRandomSalt(PWDTK.cDefaultSaltLength);
           hash = PWDTK.PasswordToHash(salt, textBox1.Text, PWDTK.cDefaultIterationCount);
           //Now store the salt and password Byte[] somewhere perhaps if WP has an implementation of iOS keychain?
           MessageBox.Show("Salt: "+PWDTK.HashBytesToHexString(salt)+"\n\nPassword: "+textBox1.Text+"\n\nHash: "+PWDTK.HashBytesToHexString(hash));
           button2.IsEnabled = true;

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (PWDTK.ComparePasswordToHash(salt, textBox1.Text, hash))
            {
                MessageBox.Show("We have a match :)\n\nSalt: " + PWDTK.HashBytesToHexString(salt) + "\n\nPassword: " + textBox1.Text + "\n\nCreated Hash: " + PWDTK.HashBytesToHexString(PWDTK.PasswordToHash(salt, textBox1.Text, PWDTK.cDefaultIterationCount)) + "\n\nStored Hash: " + PWDTK.HashBytesToHexString(hash));
            }
            else
            {
                MessageBox.Show("Hash generated doesn't match stored password :( Deny Deny Deny Deny Deny!!!!\n\nSalt: " + PWDTK.HashBytesToHexString(salt) + "\n\nPassword: " + textBox1.Text + "\n\nCreated Hash: " + PWDTK.HashBytesToHexString(PWDTK.PasswordToHash(salt, textBox1.Text, PWDTK.cDefaultIterationCount)) + "\n\nStored Hash: " + PWDTK.HashBytesToHexString(hash));
            }
        }
    }
}