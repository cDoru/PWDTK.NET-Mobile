Password Toolkit Mobile (PWDTK Mobile) v1.0.0.0 for Windows Phone

This API makes it easy to create crypto random salt and then hash salt+password via a HMACSHA256 implementation of PBKDF2.

The code also allows creation and enforcement of password policies via efficient regular expressions. An example of a password policy is forcing all user to use an upper case character and at least 2 numerics with a minimum length of 6 characters.

This code is very secure and no one is going to be feasibly creating rainbow tables for it anytime soon as just the size of the RANDOM salt alone makes rainbow tables infeasible, but in the interest of being future proof I went all out and performed key stretching as implemented in the PBKDF2 spec as well.

I have provided a very basic sample GUI which shows common usage of the API so you can see how to use it.

The package available on NuGet is made to load a compatible dll to the target project. I have provided the source for those who understandably wish to build their own dll.


Any questions or wanna say g'day etc feel free to email me (Ian) at harro84@yahoo.com.au

Thanks all!

v1.0.0.0

Initial - Based on the main project PWDTK.NET https://github.com/Thashiznets/PWDTK.NET however as Windows Phone only supports upto HMACSHA256 this is what is used.
