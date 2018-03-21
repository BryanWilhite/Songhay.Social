# Songhay.Social

![build badge](https://songhay.visualstudio.com/_apis/public/build/definitions/e6de8b87-c501-478d-8af5-b564cbd966cc/4/badge)

[Songhay System](http://songhaysystem.com) consume the public APIs of social media products in order to track promotion and engagement. This repository “rolls up” all of the hooks into these APIs into one API.

## Twitter via LinqToTwitter

As of this writing, we are using the [Twitter Search API](https://developer.twitter.com/en/docs/tweets/search/overview/standard.html) to track the “favorites” of my account, @[BryanWilhite](https://twitter.com/bryanwilhite).

The first studio application is a utility, a HTML app, that converts Twitter statuses into HTML (for archiving as Blog posts I stupidly call “twinks”—Twitter Links). For those of you reading the previous sentence for a great idea, I suggest reading, “[How to download your Twitter archive](https://help.twitter.com/en/managing-your-account/how-to-download-your-twitter-archive)” to prevent you from getting trapped in my world of misery and pain.

We are currently using LinqToTwitter [[GitHub](https://github.com/JoeMayo/LinqToTwitter)] [5.0.0-beta1](https://www.nuget.org/packages/linqtotwitter/5.0.0-beta1) to reach the Twitter APIs. We must use the beta for its compatibility with .NET Standard 2.0 and ASP.NET Core.

Having tried Buffer and [HootSuite](https://hootsuite.com/), there is an interest in “[Working with Excel in Microsoft Graph](https://developer.microsoft.com/en-us/graph/docs/api-reference/beta/resources/excel)” and [Azure Logic Apps](https://azure.microsoft.com/en-us/services/logic-apps/) to automate social engagement. I have [opened an issue](https://github.com/BryanWilhite/Songhay.Social/issues/1) to address this interest.

## Delicious (archive)

Before Twitter (and the concept of “social media”), I used [Delicious](https://en.wikipedia.org/wiki/Delicious_(website)) to track links for general-purpose, web-based research. Because Delicious _always_ provided an export facility, I was able to archive _all_ of my Delicious data and I have spent years (stupidly) converting subsets of these data into Blog posts (I called these “Delicious dumps”).

I have [opened an issue](https://github.com/BryanWilhite/Songhay.Social/issues/2) that addresses how one should go forward with this archival data (digital hoarding).

## Pinterest

The intent (for years) has been to use the [Pinterest API](https://developers.pinterest.com/docs/getting-started/introduction) to take pins from Amazon and process them for passive-income purposes. Nothing has been done with Pinterest so far.

Another currently dormant idea is around using Pinterest to drive thumbnails-view experiences (photo-gallery-like experiences) that are meant to engage visitors with content.

## Instagram

Both the Twitter API and [Instagram API](https://www.instagram.com/developer/) should be used to route content into custom experiences built in the studio. This effort would enrich the dashboard or [Index experience](https://github.com/BryanWilhite/angular.io-index-app) we are trying to build.

## GitHub

This desired Dashboard experience should be enriched by [the GitHub API](https://developer.github.com/v3/). The repository [commits](https://developer.github.com/v3/repos/commits/) and [statistics](https://developer.github.com/v3/repos/statistics/) look like a great start.

## StackOverflow

This desired Dashboard experience should be enriched by [the Stack Exchange API](https://api.stackexchange.com/docs) for StackOverflow. Starting with [badges](https://api.stackexchange.com/docs/badges) would be informative and decorative.

## LinkedIn

The [LinkedIn REST API](https://developer.linkedin.com/docs/rest-api) should enrich the Dashboard experience being built in the studio. Pulling [a member profile](https://developer.linkedin.com/docs/fields) would be a great start.

## related links

* LinqToTwitter [wiki](https://github.com/JoeMayo/LinqToTwitter/wiki)
* “[Most popular mobile social networking apps in the United States as of November 2017, by monthly users (in millions)](https://www.statista.com/statistics/248074/most-popular-us-social-networking-apps-ranked-by-audience/)”

@[BryanWilhite](https://twitter.com/bryanwilhite)