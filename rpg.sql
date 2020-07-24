-- phpMyAdmin SQL Dump
-- version 5.0.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Apr 18, 2020 at 10:03 PM
-- Server version: 10.4.11-MariaDB
-- PHP Version: 7.4.2

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `rpg`
--

-- --------------------------------------------------------

--
-- Table structure for table `accounts`
--

CREATE TABLE `accounts` (
  `id` int(11) NOT NULL,
  `username` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  `admin` int(20) NOT NULL DEFAULT 0,
  `helper` int(20) NOT NULL DEFAULT 0,
  `job` int(255) NOT NULL DEFAULT 0,
  `money` int(255) NOT NULL DEFAULT 1500,
  `materials` int(255) NOT NULL,
  `house` int(11) NOT NULL DEFAULT -1,
  `faction` int(11) NOT NULL DEFAULT 0,
  `factionRank` int(11) NOT NULL DEFAULT 0,
  `wantedLevel` int(11) NOT NULL DEFAULT 0,
  `jailTime` int(11) NOT NULL DEFAULT 0,
  `licenses` text NOT NULL DEFAULT '[]',
  `level` int(11) NOT NULL DEFAULT 1,
  `seconds` int(11) NOT NULL,
  `tempSeconds` int(11) NOT NULL,
  `xp` int(11) NOT NULL DEFAULT 0,
  `inventory` text DEFAULT '[]',
  `phoneNumber` int(11) NOT NULL DEFAULT -1,
  `timestamp` int(11) NOT NULL DEFAULT 0,
  `bankMoney` int(11) NOT NULL DEFAULT 35000,
  `bankPin` int(11) NOT NULL DEFAULT -1,
  `jobSkills` text NOT NULL DEFAULT '[]'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `accounts`
--

INSERT INTO `accounts` (`id`, `username`, `password`, `admin`, `helper`, `job`, `money`, `materials`, `house`, `faction`, `factionRank`, `wantedLevel`, `jailTime`, `licenses`, `level`, `seconds`, `tempSeconds`, `xp`, `inventory`, `phoneNumber`, `timestamp`, `bankMoney`, `bankPin`, `jobSkills`) VALUES
(1, 'denis', '1', 6, 1, 2, 4177518, 0, 38, 0, 0, 0, 0, '[{\"type\":0,\"hours\":1}]', 3, 13646, 7139, 554, '[{\"name\":\"carp\",\"count\":3,\"slot\":1,\"external\":100,\"category\":0},{\"name\":\"fishingrod\",\"count\":1,\"slot\":2,\"external\":60,\"category\":0},{\"name\":\"piranha\",\"count\":1,\"slot\":3,\"external\":0,\"category\":0},{\"name\":\"phone\",\"count\":1,\"slot\":-1,\"external\":100,\"category\":0}]', 1337, 0, 3092, 1111, '[{\"level\":1,\"xp\":0,\"job\":0},{\"level\":1,\"xp\":0,\"job\":1},{\"level\":1,\"xp\":0,\"job\":2},{\"level\":1,\"xp\":0,\"job\":3},{\"level\":5,\"xp\":175,\"job\":4},{\"level\":1,\"xp\":0,\"job\":5},{\"level\":1,\"xp\":0,\"job\":6},{\"level\":1,\"xp\":0,\"job\":7}]'),
(2, 'lightsquare', '1', 6, 1, 5, 13500, 0, 39, 1, 0, 0, 0, '[{\"type\":3,\"hours\":20},{\"type\":0,\"hours\":26}]', 2, 27376, 6865, 752, '[{\"name\":\"carp\",\"count\":2,\"slot\":1,\"external\":100,\"category\":0},{\"name\":\"fishingrod\",\"count\":1,\"slot\":2,\"external\":100,\"category\":0}]', 172885, 0, 1762, 1111, '[{\"level\":1,\"xp\":0,\"job\":0},{\"level\":1,\"xp\":0,\"job\":1},{\"level\":1,\"xp\":0,\"job\":2},{\"level\":1,\"xp\":0,\"job\":3},{\"level\":1,\"xp\":0,\"job\":4},{\"level\":4,\"xp\":0,\"job\":5},{\"level\":1,\"xp\":0,\"job\":6},{\"level\":1,\"xp\":0,\"job\":7}]'),
(3, 'olteanuadv', '1', 6, 1, 0, 97840, 0, -1, 0, 0, 0, 0, '[{\"type\":0,\"hours\":13}]', 1, 5694, 3637, 130, '[{\"name\":\"fishingrod\",\"count\":1,\"slot\":1,\"external\":100,\"category\":0},{\"name\":\"carp\",\"count\":3,\"slot\":2,\"external\":100,\"category\":0},{\"name\":\"phone\",\"count\":1,\"slot\":3,\"external\":100,\"category\":0},{\"name\":\"phonebook\",\"count\":1,\"slot\":4,\"external\":100,\"category\":0}]', -1, 0, 5078610, 1111, '');

-- --------------------------------------------------------

--
-- Table structure for table `business`
--

CREATE TABLE `business` (
  `id` int(11) NOT NULL,
  `owner` text NOT NULL,
  `name` text NOT NULL,
  `enterPos` text NOT NULL DEFAULT '{x:0,y:0,z:0}',
  `exitPos` text NOT NULL DEFAULT '{x:0,y:0,z:0}',
  `type` int(11) NOT NULL,
  `external` text NOT NULL DEFAULT '[]',
  `sale` int(11) NOT NULL DEFAULT 0,
  `balance` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `business`
--

INSERT INTO `business` (`id`, `owner`, `name`, `enterPos`, `exitPos`, `type`, `external`, `sale`, `balance`) VALUES
(14, 'denis', 'cumparal teo', '{\"x\":243.75745,\"y\":-45.012196,\"z\":69.896576}', '{x:0,y:0,z:0}', 2, '{\"buyPosition\":{\"x\":252.15338,\"y\":-49.821346,\"z\":69.94105},\"gunPosition\":{\"x\":253.21582,\"y\":-50.21849,\"z\":70.93771},\"gunRotationZ\":-110.62627,\"pedPosition\":{\"x\":254.4221,\"y\":-50.622135,\"z\":69.94102}}', 0, 14),
(15, 'denis', '', '{\"x\":-1112.4039,\"y\":2690.0205,\"z\":18.589003}', '{x:0,y:0,z:0}', 2, '{\"buyPosition\":{\"x\":-1117.8384,\"y\":2698.0298,\"z\":18.55415},\"gunPosition\":{\"x\":-1118.6245,\"y\":2699.1304,\"z\":19.549187},\"gunRotationZ\":38.056267,\"pedPosition\":{\"x\":-1119.6329,\"y\":2700.091,\"z\":18.55413},\"pedHeading\":-139.36009}', 0, 0),
(16, 'AdmBot', '', '{\"x\":17.167799,\"y\":-1115.5629,\"z\":29.79118}', '{x:0,y:0,z:0}', 2, '{\"buyPosition\":{\"x\":21.605806,\"y\":-1107.0365,\"z\":29.797026},\"gunPosition\":{\"x\":22.04393,\"y\":-1106.0479,\"z\":30.793648},\"gunRotationZ\":-20.098936,\"pedPosition\":{\"x\":22.523216,\"y\":-1105.0876,\"z\":29.797007},\"pedHeading\":149.19289}', 0, 2),
(17, 'AdmCmd', 'DowntownVinewood', '{\"x\":376.58105,\"y\":323.15454,\"z\":103.572815}', '{x:0,y:0,z:0}', 4, '{\"checkin\":{\"x\":378.56818,\"y\":323.7151,\"z\":103.56647},\"checkout\":{\"x\":374.30087,\"y\":328.11948,\"z\":103.56647},\"rafts\":[{\"name\":\"Raft1\",\"position\":{\"x\":375.96863,\"y\":327.71576,\"z\":103.56645},\"items\":[{\"price\":1000,\"type\":\"fishingrod\",\"count\":1},{\"price\":1000,\"type\":\"carp\",\"count\":1}]},{\"name\":\"Raft1\",\"position\":{\"x\":378.6869,\"y\":326.74582,\"z\":103.56645},\"items\":[{\"price\":500,\"type\":\"phone\",\"count\":1},{\"price\":500,\"type\":\"phonebook\",\"count\":1}]}],\"center\":{\"x\":377.96613,\"y\":327.41315,\"z\":103.56645}}', 0, 62),
(19, 'AdmBot', 'Pacific', '{\"x\":231.76839,\"y\":215.19551,\"z\":106.28018}', '{x:0,y:0,z:0}', 0, '{\"atms\":[{\"position\":{\"x\":237.26291,\"y\":217.78499,\"z\":106.28677}}],\"counters\":[{\"pedPosition\":{\"x\":241.92531,\"y\":227.20279,\"z\":106.27842},\"pedRotation\":161.20782,\"textLabel\":{\"x\":241.51782,\"y\":225.29819,\"z\":106.28683}}]}', 150000, 26725),
(20, 'AdmBot', 'Downtown', '{\"x\":619.8774,\"y\":242.88425,\"z\":103.16661}', '{x:0,y:0,z:0}', 1, '{\"stations\":[{\"position\":{\"x\":614.2869,\"y\":263.53268,\"z\":103.089386}},{\"position\":{\"x\":614.1504,\"y\":272.24805,\"z\":103.089386}},{\"position\":{\"x\":619.5905,\"y\":273.82812,\"z\":103.08948}},{\"position\":{\"x\":619.4578,\"y\":264.90323,\"z\":103.08948}},{\"position\":{\"x\":622.71375,\"y\":263.30658,\"z\":103.089386}},{\"position\":{\"x\":622.6031,\"y\":272.91702,\"z\":103.089386}}],\"isShop\":false,\"center\":{\"x\":619.22894,\"y\":265.9183,\"z\":103.089455},\"pedPosition\":{\"x\":642.1074,\"y\":260.91547,\"z\":103.295586}}', 0, 0);

-- --------------------------------------------------------

--
-- Table structure for table `busroutes`
--

CREATE TABLE `busroutes` (
  `id` int(11) NOT NULL,
  `name` text NOT NULL,
  `points` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `busroutes`
--

INSERT INTO `busroutes` (`id`, `name`, `points`) VALUES
(2, 'sd', '[{\"position\":{\"x\":-426.64368,\"y\":1141.6917,\"z\":325.9045},\"name\":\"statie1\"},{\"position\":{\"x\":-432.24017,\"y\":1151.2603,\"z\":325.90454},\"name\":\"statie2\"},{\"position\":{\"x\":-422.8984,\"y\":1159.7677,\"z\":325.9051},\"name\":\"statie3\"},{\"position\":{\"x\":-406.3307,\"y\":1157.3982,\"z\":325.90976},\"name\":\"statie4\"},{\"position\":{\"x\":-407.8846,\"y\":1147.775,\"z\":325.906},\"name\":\"statie5\"}]'),
(3, 'bulevard', '[{\"position\":{\"x\":-376.9996,\"y\":1170.9233,\"z\":325.2366},\"name\":\"a\"},{\"position\":{\"x\":-337.53815,\"y\":1172.7174,\"z\":324.05945},\"name\":\"a\"},{\"position\":{\"x\":-306.96112,\"y\":1199.3672,\"z\":320.14896},\"name\":\"a\"},{\"position\":{\"x\":-275.78107,\"y\":1224.5828,\"z\":316.40918},\"name\":\"a\"},{\"position\":{\"x\":-258.38538,\"y\":1263.9418,\"z\":310.55997},\"name\":\"a\"},{\"position\":{\"x\":-232.48291,\"y\":1287.7455,\"z\":306.7412},\"name\":\"a\"},{\"position\":{\"x\":-204.5628,\"y\":1307.26,\"z\":304.3107},\"name\":\"a\"},{\"position\":{\"x\":-186.94586,\"y\":1328.8613,\"z\":301.35605},\"name\":\"a\"},{\"position\":{\"x\":-179.82904,\"y\":1360.8188,\"z\":296.58115},\"name\":\"a\"},{\"position\":{\"x\":-188.336,\"y\":1394.3156,\"z\":292.46503},\"name\":\"a\"},{\"position\":{\"x\":-195.39536,\"y\":1427.0847,\"z\":289.40118},\"name\":\"a\"},{\"position\":{\"x\":-192.92978,\"y\":1458.0862,\"z\":288.35355},\"name\":\"a\"},{\"position\":{\"x\":-183.5209,\"y\":1481.5149,\"z\":288.3801},\"name\":\"a\"},{\"position\":{\"x\":-146.152,\"y\":1506.727,\"z\":288.1727},\"name\":\"a\"},{\"position\":{\"x\":-117.39995,\"y\":1509.5657,\"z\":285.92886},\"name\":\"a\"},{\"position\":{\"x\":-84.504616,\"y\":1501.6835,\"z\":282.3946},\"name\":\"a\"}]'),
(4, 'VinewoodHills', '[{\"position\":{\"x\":420.5728,\"y\":-673.6866,\"z\":28.617987},\"name\":\"Dashhound\"},{\"position\":{\"x\":-36.017876,\"y\":-502.9358,\"z\":39.755157},\"name\":\"Arcadius\"},{\"position\":{\"x\":-10.108811,\"y\":-255.76831,\"z\":46.140644},\"name\":\"RockfordPlaza\"},{\"position\":{\"x\":-142.77888,\"y\":-73.40261,\"z\":54.125126},\"name\":\"Yeti\"},{\"position\":{\"x\":-504.27493,\"y\":20.022665,\"z\":44.143326},\"name\":\"OrganicHealth\"},{\"position\":{\"x\":-754.0136,\"y\":-34.690376,\"z\":37.089874},\"name\":\"RockfordHills\"},{\"position\":{\"x\":-930.33875,\"y\":-126.02891,\"z\":36.991997},\"name\":\"GolfingSociety\"},{\"position\":{\"x\":-1247.7595,\"y\":-304.24695,\"z\":36.6419},\"name\":\"Morningwood\"},{\"position\":{\"x\":-1525.301,\"y\":-467.97293,\"z\":34.77513},\"name\":\"DelPerroTowers\"},{\"position\":{\"x\":-1618.7201,\"y\":-530.9232,\"z\":33.874912},\"name\":\"BannerHotel\"},{\"position\":{\"x\":-1583.787,\"y\":-650.22314,\"z\":29.11846},\"name\":\"DelPerroBeatch\"},{\"position\":{\"x\":-1277.9999,\"y\":-992.3296,\"z\":9.240013},\"name\":\"FiveHotels\"},{\"position\":{\"x\":-1212.9814,\"y\":-1216.9755,\"z\":6.984384},\"name\":\"Vespucci\"},{\"position\":{\"x\":-1107.9952,\"y\":-1327.2625,\"z\":4.466123},\"name\":\"LaSpada\"},{\"position\":{\"x\":-823.0307,\"y\":-1163.2971,\"z\":6.7263527},\"name\":\"TheViceroy\"},{\"position\":{\"x\":-651.3851,\"y\":-1047.1677,\"z\":16.731703},\"name\":\"LittleSeoul\"},{\"position\":{\"x\":-547.1109,\"y\":-1020.4983,\"z\":22.167816},\"name\":\"WeazelNews\"},{\"position\":{\"x\":-341.68127,\"y\":-1146.5432,\"z\":26.47266},\"name\":\"ConstructionPoint\"},{\"position\":{\"x\":-176.30289,\"y\":-1150.2538,\"z\":22.41226},\"name\":\"Strawberry\"},{\"position\":{\"x\":18.613657,\"y\":-1146.1686,\"z\":28.359533},\"name\":\"LSGC\"},{\"position\":{\"x\":151.46173,\"y\":-913.47345,\"z\":29.457298},\"name\":\"LegionSquare\"},{\"position\":{\"x\":325.57062,\"y\":-786.1097,\"z\":28.627916},\"name\":\"LSPD\"},{\"position\":{\"x\":444.14487,\"y\":-682.1584,\"z\":27.866547},\"name\":\"SimmetAlley\"}]');

-- --------------------------------------------------------

--
-- Table structure for table `contracts`
--

CREATE TABLE `contracts` (
  `id` int(11) NOT NULL,
  `hitman` text NOT NULL,
  `target` text NOT NULL,
  `Player` text NOT NULL,
  `money` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Table structure for table `data`
--

CREATE TABLE `data` (
  `weather` text NOT NULL,
  `lastWeatherUpdate` text NOT NULL,
  `id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `data`
--

INSERT INTO `data` (`weather`, `lastWeatherUpdate`, `id`) VALUES
('rain', '4/18/2020 10:22:10 PM', 2);

-- --------------------------------------------------------

--
-- Table structure for table `factions`
--

CREATE TABLE `factions` (
  `id` int(11) NOT NULL,
  `maxMembers` int(11) NOT NULL DEFAULT 30,
  `membersCount` int(11) NOT NULL DEFAULT 0,
  `factionMembers` text NOT NULL DEFAULT '[]',
  `enterHq` text NOT NULL DEFAULT '[x: 0, y: 0, z: 0]',
  `exitHq` text NOT NULL DEFAULT '[x: 0, y: 0, z: 0]',
  `ipl` text NOT NULL DEFAULT 'none',
  `blip` int(11) NOT NULL DEFAULT 0,
  `locked` tinyint(1) NOT NULL,
  `name` text NOT NULL,
  `type` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `factions`
--

INSERT INTO `factions` (`id`, `maxMembers`, `membersCount`, `factionMembers`, `enterHq`, `exitHq`, `ipl`, `blip`, `locked`, `name`, `type`) VALUES
(1, 30, 0, '[]', '{x:433.24393, y:-982.09484, z:30.710026}', '{x: 0, y: 0, z: 0}', 'none', 60, 0, 'Los Santos Police Department', 1),
(3, 30, 0, '[]', '{x:2119.017, y:3331.2695, z:46.748173}', '{x:2154.3296, y:2921.0771, z:-81.07546}', 'none', 671, 0, 'Hitman Agency', 2);

-- --------------------------------------------------------

--
-- Table structure for table `garbagepositions`
--

CREATE TABLE `garbagepositions` (
  `id` int(11) NOT NULL,
  `position` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `garbagepositions`
--

INSERT INTO `garbagepositions` (`id`, `position`) VALUES
(12, '{\"x\":-540.3989,\"y\":-2203.0095,\"z\":5.974023}'),
(13, '{\"x\":-544.4313,\"y\":-2228.5579,\"z\":6.394024}'),
(14, '{\"x\":-549.1486,\"y\":-2246.4072,\"z\":6.128283}');

-- --------------------------------------------------------

--
-- Table structure for table `houses`
--

CREATE TABLE `houses` (
  `id` int(11) NOT NULL,
  `owner` text NOT NULL,
  `enterPos` text NOT NULL,
  `exitPos` text NOT NULL,
  `sale` int(11) NOT NULL,
  `rentPrice` int(11) NOT NULL,
  `size` int(11) NOT NULL,
  `description` text NOT NULL,
  `locked` tinyint(1) NOT NULL,
  `ipl` text NOT NULL,
  `renters` text NOT NULL DEFAULT '[]'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `houses`
--

INSERT INTO `houses` (`id`, `owner`, `enterPos`, `exitPos`, `sale`, `rentPrice`, `size`, `description`, `locked`, `ipl`, `renters`) VALUES
(38, 'denis', '{\"x\":-1366.0193,\"y\":56.666626,\"z\":54.09845}', '{\"x\":340.9412,\"y\":437.1798,\"z\":149.3925}', 0, 1500, 2, 'Casa civililor', 0, 'noIPL', '[]'),
(39, 'lightsquare', '{\"x\":-14.211779,\"y\":-1441.8452,\"z\":31.101572}', '{\"x\":-14.189161,\"y\":-1440.3384,\"z\":31.101555}', 0, 0, 0, 'House', 0, 'noIPL', '[]');

-- --------------------------------------------------------

--
-- Table structure for table `mdc`
--

CREATE TABLE `mdc` (
  `id` int(11) NOT NULL,
  `cop` text NOT NULL,
  `suspect` text NOT NULL,
  `wantedLevel` int(11) NOT NULL,
  `wantedReason` text NOT NULL,
  `wantedTime` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `mdc`
--

INSERT INTO `mdc` (`id`, `cop`, `suspect`, `wantedLevel`, `wantedReason`, `wantedTime`) VALUES
(77, 'clau', 'denis', 2, 'tulburarea linistii publice', '2020-04-02 22:55:48');

-- --------------------------------------------------------

--
-- Table structure for table `trailerspark`
--

CREATE TABLE `trailerspark` (
  `id` int(11) NOT NULL,
  `position` text NOT NULL DEFAULT '{x:0, y:0, z:0}',
  `rotation` float NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `trailerspark`
--

INSERT INTO `trailerspark` (`id`, `position`, `rotation`) VALUES
(1, '{\"x\":892.7327,\"y\":-3130.244,\"z\":5.4061174}', 0),
(2, '{\"x\":896.6441,\"y\":-3130.4895,\"z\":5.4059434}', 0),
(3, '{\"x\":900.6963,\"y\":-3131.5427,\"z\":5.4058366}', 0),
(4, '{\"x\":904.8751,\"y\":-3132.0935,\"z\":5.4059367}', 0),
(5, '{\"x\":908.71295,\"y\":-3133.106,\"z\":5.405998}', 0),
(6, '{\"x\":912.95917,\"y\":-3131.1897,\"z\":5.406722}', 0),
(7, '{\"x\":917.0019,\"y\":-3131.8186,\"z\":5.405941}', 0),
(8, '{\"x\":921.0446,\"y\":-3130.5137,\"z\":5.4071965}', 0),
(9, '{\"x\":925.12213,\"y\":-3131.7043,\"z\":5.406569}', 0),
(10, '{\"x\":929.04144,\"y\":-3132.637,\"z\":5.406269}', 0),
(11, '{\"x\":933.3405,\"y\":-3130.358,\"z\":5.4060087}', 0),
(12, '{\"x\":937.21533,\"y\":-3130.9573,\"z\":5.405964}', 0),
(13, '{\"x\":941.22797,\"y\":-3129.5361,\"z\":5.405937}', 0),
(14, '{\"x\":945.39435,\"y\":-3128.796,\"z\":5.4060326}', 0),
(15, '{\"x\":949.43567,\"y\":-3130.1003,\"z\":5.406074}', 0),
(16, '{\"x\":953.536,\"y\":-3131.0486,\"z\":5.405363}', 0),
(17, '{\"x\":957.5926,\"y\":-3129.4126,\"z\":5.4057784}', 0),
(18, '{\"x\":961.5498,\"y\":-3128.969,\"z\":5.4070044}', 0),
(19, '{\"x\":965.65576,\"y\":-3129.0906,\"z\":5.4061418}', 0),
(20, '{\"x\":969.7407,\"y\":-3128.683,\"z\":5.406068}', 0);

-- --------------------------------------------------------

--
-- Table structure for table `truckerroutes`
--

CREATE TABLE `truckerroutes` (
  `id` int(11) NOT NULL,
  `distance` float NOT NULL DEFAULT 0,
  `destination` text NOT NULL,
  `price` int(11) NOT NULL DEFAULT 0,
  `description` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `truckerroutes`
--

INSERT INTO `truckerroutes` (`id`, `distance`, `destination`, `price`, `description`) VALUES
(3, 1656.54, '{\"location\":{\"x\":780.3324,\"y\":-2965.1304,\"z\":5.8007174},\"name\":\"Casino-4-dragoni\"}', 59, '');

-- --------------------------------------------------------

--
-- Table structure for table `vehicles`
--

CREATE TABLE `vehicles` (
  `id` int(11) NOT NULL,
  `owner` text NOT NULL,
  `model` text NOT NULL,
  `type` int(11) NOT NULL,
  `spawnPosition` text NOT NULL,
  `primaryColor` int(11) NOT NULL,
  `secondaryColor` int(11) NOT NULL,
  `numberPlate` text NOT NULL,
  `fuel` int(11) NOT NULL DEFAULT 0,
  `kms` int(11) NOT NULL DEFAULT 0,
  `insurances` int(1) NOT NULL DEFAULT 0,
  `locked` tinyint(1) NOT NULL DEFAULT 0,
  `rotation` float NOT NULL DEFAULT 0,
  `creationDate` date NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `vehicles`
--

INSERT INTO `vehicles` (`id`, `owner`, `model`, `type`, `spawnPosition`, `primaryColor`, `secondaryColor`, `numberPlate`, `fuel`, `kms`, `insurances`, `locked`, `rotation`, `creationDate`) VALUES
(1, 'job', 'tractor2', 3, '{\"x\":1216.9761,\"y\":1819.9446,\"z\":79.02005}', 88, 88, 'job', 0, 0, 0, 0, 0, '2020-03-18'),
(2, 'ATHBUS', 'coach', 2, '{\"x\":456.6915,\"y\":-653.4817,\"z\":28.741692}', 75, 24, 'ATHBUS', 0, 0, 0, 0, -146.486, '2020-03-18'),
(5, 'ATHBUS', 'coach', 2, '{\"x\":457.671,\"y\":-646.50024,\"z\":29.117157}', 75, 24, 'ATHBUS', 0, 0, 0, 0, -146.984, '2020-03-18'),
(6, 'ATHBUS', 'coach', 2, '{\"x\":458.4696,\"y\":-639.20013,\"z\":29.32536}', 75, 24, 'ATHBUS', 0, 0, 0, 0, -146.486, '2020-03-18'),
(7, 'ATHBUS', 'coach', 2, '{\"x\":458.82608,\"y\":-631.89417,\"z\":29.330729}', 75, 24, 'ATHBUS', 0, 0, 0, 0, -146.738, '2020-03-18'),
(8, 'ATHBUS', 'coach', 2, '{\"x\":459.6782,\"y\":-624.7695,\"z\":29.330578}', 75, 24, 'ATHBUS', 0, 0, 0, 0, -147.006, '2020-03-18'),
(9, 'ATHBUS', 'coach', 2, '{\"x\":460.32187,\"y\":-617.71686,\"z\":29.330698}', 75, 24, 'ATHBUS', 0, 0, 0, 0, -145.923, '2020-03-18'),
(10, 'ATHBUS', 'coach', 2, '{\"x\":461.24005,\"y\":-610.4631,\"z\":29.330046}', 75, 24, 'ATHBUS', 0, 0, 0, 0, -146.04, '2020-03-18'),
(11, 'ATHBUS', 'coach', 2, '{\"x\":462.1243,\"y\":-604.4204,\"z\":29.331125}', 75, 24, 'ATHBUS', 0, 0, 0, 0, -147.762, '2020-03-18'),
(27, 'clau', 'buffalo2', 1, '{\"x\":-735.66315,\"y\":4.5324316,\"z\":37.473698}', 30, 30, 'ATH', 30, 5, 2, 0, 27.13, '2019-03-18'),
(28, 'clau', 'sultan', 1, '{\"x\":-47.8021,\"y\":-1116.419,\"z\":26.43427}', 0, 0, 'LS-3145', 0, 0, 0, 0, 0, '2020-03-18'),
(29, 'clau', 'elegy2', 1, '{\"x\":587.56537,\"y\":253.21307,\"z\":102.955086}', 5, 7, 'ATH', 0, 0, 0, 0, 0, '2020-03-18'),
(30, 'clau', 'Sultan', 0, '{\"x\":-47.8021,\"y\":-1116.419,\"z\":26.43427}', 0, 0, 'LS-3145', 0, 0, 0, 0, 0, '2020-03-19'),
(31, 'clau', 'Sultan', 0, '{\"x\":-50.66175,\"y\":-1116.753,\"z\":26.4342}', 89, 35, 'LS-3145', 0, 0, 0, 0, 0, '2020-03-19'),
(32, 'AS', 'Sultan', 0, '{\"x\":-755.34625,\"y\":-3.9972508,\"z\":40.26204}', 22, 33, 'AS', 0, 0, 0, 0, 124.951, '2020-03-21'),
(33, 'AS', 'Sultan', 0, '{\"x\":-750.1877,\"y\":-3.2907827,\"z\":39.735874}', 22, 33, 'AS', 0, 0, 0, 0, -6.74903, '2020-03-21'),
(34, 'AS', 'Sultan', 0, '{\"x\":-742.85114,\"y\":-6.04846,\"z\":38.899834}', 22, 33, 'AS', 0, 0, 0, 0, 152.241, '2020-03-21'),
(35, 'AS', 'Sultan', 0, '{\"x\":-737.4442,\"y\":-9.348658,\"z\":38.002106}', 22, 33, 'AS', 0, 0, 0, 0, -96.0388, '2020-03-21'),
(36, 'AS', 'Sultan', 0, '{\"x\":-731.6144,\"y\":-4.5381856,\"z\":37.852898}', 22, 33, 'AS', 0, 0, 0, 0, -169.635, '2020-03-21'),
(37, 'DMV', 'asbo', 6, '{\"x\":-727.564,\"y\":-69.26398,\"z\":41.142555}', 24, 27, 'DMV', 0, 0, 0, 0, 26.4703, '2020-03-22'),
(38, 'DMV', 'asbo', 6, '{\"x\":-730.34204,\"y\":-70.76917,\"z\":41.140957}', 4, 135, 'DMV', 0, 0, 0, 0, 26.5593, '2020-03-22'),
(39, 'DMV', 'asbo', 6, '{\"x\":-733.05914,\"y\":-72.11708,\"z\":41.13999}', 73, 150, 'DMV', 0, 0, 0, 0, 26.6423, '2020-03-22'),
(40, 'ATH', 'caddy', 100, '{\"x\":-1356.9005,\"y\":130.22433,\"z\":55.637333}', 112, 112, 'ATH', 0, 0, 0, 0, -84.3178, '2020-03-26'),
(41, 'ATH', 'caddy', 100, '{\"x\":-1357.5709,\"y\":133.59274,\"z\":55.646854}', 32, 32, 'ATH', 0, 0, 0, 0, -86.0902, '2020-03-26'),
(42, 'ATH', 'caddy2', 100, '{\"x\":-1344.1221,\"y\":135.66628,\"z\":55.64463}', 32, 32, 'ATH', 0, 0, 0, 0, 95.0299, '2020-03-26'),
(47, 'ATH', 'trash', 5, '{\"x\":-497.87802,\"y\":-2197.6694,\"z\":6.277181}', 158, 158, 'ATH', 0, 0, 0, 0, 47.3407, '2020-03-28'),
(48, 'ATH', 'trash', 5, '{\"x\":-495.44403,\"y\":-2195.0823,\"z\":6.599985}', 128, 128, 'ATH', 0, 0, 0, 0, 48.0203, '2020-03-28'),
(49, 'ATH', 'trash', 5, '{\"x\":-493.1038,\"y\":-2192.4395,\"z\":7.04449}', 128, 128, 'ATH', 0, 0, 0, 0, 49.5252, '2020-03-28'),
(50, 'ATH', 'trash', 5, '{\"x\":-490.78934,\"y\":-2189.581,\"z\":7.550318}', 158, 158, 'ATH', 0, 0, 0, 0, 49.8754, '2020-03-28'),
(51, 'ATH', 'trash', 5, '{\"x\":-488.23172,\"y\":-2186.3833,\"z\":8.132925}', 158, 158, 'ATH', 0, 0, 0, 0, 48.8257, '2020-03-28'),
(52, 'ATH', 'trash', 5, '{\"x\":-485.68332,\"y\":-2183.4768,\"z\":8.522181}', 128, 128, 'ATH', 0, 0, 0, 0, 47.9827, '2020-03-28'),
(53, 'ATH', 'trash', 5, '{\"x\":-517.707,\"y\":-2184.9048,\"z\":6.112223}', 128, 128, 'ATH', 0, 0, 0, 0, -129.935, '2020-03-28'),
(54, 'ATH', 'phantom', 7, '{\"x\":817.1323,\"y\":-3225.3274,\"z\":5.966551}', 1, 1, 'ATH', 0, 0, 0, 0, 0.066649, '2020-03-30'),
(55, 'ATH', 'faggio2', 4, '{\"x\":-1174.1521,\"y\":-872.8934,\"z\":13.624155}', 5, 6, 'ATH', 0, 0, 0, 0, 125.365, '2020-04-08'),
(56, 'ATH', 'faggio2', 4, '{\"x\":-1173.1458,\"y\":-876.8236,\"z\":13.594902}', 5, 6, 'ATH', 0, 0, 0, 0, 126.813, '2020-04-08'),
(57, 'ATH', 'faggio2', 4, '{\"x\":-1170.2275,\"y\":-879.6322,\"z\":13.624236}', 5, 6, 'ATH', 0, 0, 0, 0, 124.127, '2020-04-08'),
(58, 'ATH', 'faggio2', 4, '{\"x\":-1168.163,\"y\":-882.85284,\"z\":13.629025}', 5, 6, 'ATH', 0, 0, 0, 0, 125.954, '2020-04-08'),
(59, 'ATH', 'faggio2', 4, '{\"x\":-1165.9545,\"y\":-888.25934,\"z\":13.603446}', 5, 6, 'ATH', 0, 0, 0, 0, 120.546, '2020-04-08'),
(60, 'ATH', 'faggio2', 4, '{\"x\":-1164.0269,\"y\":-891.4633,\"z\":13.601958}', 5, 6, 'ATH', 0, 0, 0, 0, 113.883, '2020-04-08'),
(61, 'denis', 'Infernus', 1, '{\"x\":-53.51776,\"y\":-1116.721,\"z\":26.43449}', 0, 0, 'LS-3145', 0, 0, 3, 0, 0, '2020-04-09'),
(62, 'olteanuadv', 'Infernus', 1, '{\"x\":-56.41209,\"y\":-1116.901,\"z\":26.43442}', 156, 0, 'LS-3145', 0, 0, 4, 0, 0, '2020-04-13');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `accounts`
--
ALTER TABLE `accounts`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `business`
--
ALTER TABLE `business`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `busroutes`
--
ALTER TABLE `busroutes`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `contracts`
--
ALTER TABLE `contracts`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `data`
--
ALTER TABLE `data`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `factions`
--
ALTER TABLE `factions`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `garbagepositions`
--
ALTER TABLE `garbagepositions`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `houses`
--
ALTER TABLE `houses`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `mdc`
--
ALTER TABLE `mdc`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `trailerspark`
--
ALTER TABLE `trailerspark`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `truckerroutes`
--
ALTER TABLE `truckerroutes`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `vehicles`
--
ALTER TABLE `vehicles`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `accounts`
--
ALTER TABLE `accounts`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `business`
--
ALTER TABLE `business`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=21;

--
-- AUTO_INCREMENT for table `busroutes`
--
ALTER TABLE `busroutes`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT for table `contracts`
--
ALTER TABLE `contracts`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `data`
--
ALTER TABLE `data`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT for table `factions`
--
ALTER TABLE `factions`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `garbagepositions`
--
ALTER TABLE `garbagepositions`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=15;

--
-- AUTO_INCREMENT for table `houses`
--
ALTER TABLE `houses`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=40;

--
-- AUTO_INCREMENT for table `mdc`
--
ALTER TABLE `mdc`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=78;

--
-- AUTO_INCREMENT for table `trailerspark`
--
ALTER TABLE `trailerspark`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=21;

--
-- AUTO_INCREMENT for table `truckerroutes`
--
ALTER TABLE `truckerroutes`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `vehicles`
--
ALTER TABLE `vehicles`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=63;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
