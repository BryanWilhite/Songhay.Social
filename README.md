# Songhay.Social

[Songhay System](http://songhaysystem.com) consume the public APIs of social media products in order to track promotion and engagement. This repository “rolls up” all of the hooks into these APIs into one API.

## Twitter via LinqToTwitter

As of this writing, we are using the [Twitter Search API](https://developer.twitter.com/en/docs/tweets/search/overview/standard.html) to track the “favorites” of my account, @[BryanWilhite](https://twitter.com/bryanwilhite).

The first studio application is a utility, a HTML app, that converts Twitter statuses into HTML (for archiving as Blog posts I stupidly call “twinks”—Twitter Links). For those of you reading the previous sentence for a great idea, I suggest reading, “[How to download your Twitter archive](https://help.twitter.com/en/managing-your-account/how-to-download-your-twitter-archive)” to prevent you from getting trapped in my world of misery and pain.

We are currently using LinqToTwitter [[GitHub](https://github.com/JoeMayo/LinqToTwitter)] [5.0.0-beta1](https://www.nuget.org/packages/linqtotwitter/5.0.0-beta1) to reach the Twitter APIs. We must use the beta for its compatibility with .NET Standard 2.0 and ASP.NET Core.

## Delicious (archive)

Before Twitter (and the concept of “social media”), I used [Delicious](https://en.wikipedia.org/wiki/Delicious_(website)) to track links for general-purpose, web-based research. Because Delicious _always_ provided an export facility, I was able to archive _all_ of my Delicious data and I have spent years (stupidly) converting subsets of these data into Blog posts (I called these “Delicious dumps”).

I have [opened an issue](https://github.com/BryanWilhite/Songhay.Social/issues/2) that addresses how one should go forward with this archival data (digital hoarding).

## Pinterest

The intent (for years) has been to use the [Pinterest API](https://developers.pinterest.com/docs/getting-started/introduction) to take pins from Amazon and process them for passive-income purposes. Nothing has been done with Pinterest so far.

Another currently dormant idea is around using Pinterest to drive thumbnails-view experiences (photo-gallery-like experiences) that are meant to engage visitors with content.

## Instagram

Both the Twitter API and [Instagram API](https://www.instagram.com/developer/) should be used to route content into custom experiences built in the studio. This effort would enrich the dashboard or [Index experience](https://github.com/BryanWilhite/angular.io-index-app) we are trying to build.

## related links

* “[Most popular mobile social networking apps in the United States as of November 2017, by monthly users (in millions)](https://www.statista.com/statistics/248074/most-popular-us-social-networking-apps-ranked-by-audience/)”

@[BryanWilhite](https://twitter.com/bryanwilhite)