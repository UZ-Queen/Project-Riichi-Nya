using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


    public enum Yaku
    {
        //1판역
        Riichi, Tanyao, Tsumo,
        Ton,Nan,Sha,Pei,
        DoubleTon, DoubleNan, DoubleSha, DoublePei, 
        Haku, Hatsu, Chun,
        Pinfu, Ipeko,
        ChanKkan, Rinshan,
        Haitei, Houtei, Itpatsu,
        //2판역
        DoubleRiichi, SansyokuDoko, SanKkanzu,
        ToiToi, SanAnkou, Syoshangen, Honnodou,
        Chittoi, Chanta, Ittsu, SamsaekDounsun,
        //3판역
        Ryanpeko, Junchan, Honiisou, 
        //6판역
        Chiniisou,

        //야꾸만
        Tenhou, Chihou,
        Daisangen, SsuAnkou,
        Tsuuiisou, Ryuuiisou, Chinroutou, 
        Kokushimusou,
        Syosushi, Daisushi, 
        Ssukanzu, Chuuren, 
        //도라네코마츠리냐
        Dora, AkaDora, UraDora, NukiDora, 
        //역 없음
        Danbean,

    }

    public class YakuInfo
    {
        public enum Condition { MenzenOnly, DecreaseHanWhenFuro, FuroOK};

        public readonly Yaku yaku;
        public int Han { get; }
        Condition condition { get; }
        public SortedSet<Yaku> lowerYakues; // 이 역이 성립한다면, 다음 역들은 무시된다
                                            //public string FlavorText { get; }
                                            //예: 치또이는 량페코를 무시한다. 량페코도 치또이를 무시한다.
                                            // 청일색은 혼일색을 무시한다.

        public static readonly Dictionary<Yaku, YakuInfo> YakuData = new Dictionary<Yaku, YakuInfo>
        {
            // 1판역
            {Yaku.Riichi, new YakuInfo(Yaku.Riichi, 1, Condition.MenzenOnly)},
            {Yaku.Tanyao, new YakuInfo(Yaku.Tanyao, 1, Condition.FuroOK)},
            {Yaku.Tsumo, new YakuInfo(Yaku.Tsumo, 1, Condition.MenzenOnly)},
            {Yaku.Ton, new YakuInfo(Yaku.Ton, 1, Condition.FuroOK)},
            {Yaku.Nan, new YakuInfo(Yaku.Nan, 1, Condition.FuroOK)},
            {Yaku.Sha, new YakuInfo(Yaku.Sha, 1, Condition.FuroOK)},
            {Yaku.Pei, new YakuInfo(Yaku.Pei, 1, Condition.FuroOK)},
            {Yaku.DoubleTon, new YakuInfo(Yaku.DoubleTon, 1, Condition.FuroOK)},
            {Yaku.DoubleNan, new YakuInfo(Yaku.DoubleNan, 1, Condition.FuroOK)},
            {Yaku.DoubleSha, new YakuInfo(Yaku.DoubleSha, 1, Condition.FuroOK)},
            {Yaku.DoublePei, new YakuInfo(Yaku.DoublePei, 1, Condition.FuroOK)},
            {Yaku.Haku, new YakuInfo(Yaku.Haku, 1, Condition.FuroOK)},
            {Yaku.Hatsu, new YakuInfo(Yaku.Hatsu, 1, Condition.FuroOK)},
            {Yaku.Chun, new YakuInfo(Yaku.Chun, 1, Condition.FuroOK)},

            {Yaku.Pinfu, new YakuInfo(Yaku.Pinfu, 1, Condition.MenzenOnly)},
            {Yaku.Ipeko, new YakuInfo(Yaku.Ipeko, 1, Condition.MenzenOnly)},

            {Yaku.ChanKkan, new YakuInfo(Yaku.ChanKkan, 1, Condition.FuroOK)},
            {Yaku.Rinshan, new YakuInfo(Yaku.Rinshan, 1, Condition.FuroOK)},
            {Yaku.Haitei, new YakuInfo(Yaku.Haitei, 1, Condition.FuroOK)},
            {Yaku.Houtei, new YakuInfo(Yaku.Houtei, 1, Condition.FuroOK)},
            {Yaku.Itpatsu, new YakuInfo(Yaku.Itpatsu, 1, Condition.MenzenOnly)},

            // 2판역
            {Yaku.DoubleRiichi, new YakuInfo(Yaku.DoubleRiichi, 2, Condition.MenzenOnly, YakuSet(Yaku.Riichi))},
            {Yaku.SansyokuDoko, new YakuInfo(Yaku.SansyokuDoko, 2, Condition.FuroOK)},
            {Yaku.SanKkanzu, new YakuInfo(Yaku.SanKkanzu, 2, Condition.FuroOK)},
            {Yaku.ToiToi, new YakuInfo(Yaku.ToiToi, 2, Condition.FuroOK)},
            {Yaku.SanAnkou, new YakuInfo(Yaku.SanAnkou, 2, Condition.FuroOK)},
            {Yaku.Syoshangen, new YakuInfo(Yaku.Syoshangen, 2, Condition.FuroOK)},
            {Yaku.Honnodou, new YakuInfo(Yaku.Honnodou, 2, Condition.FuroOK, YakuSet(Yaku.Chanta))},
            {Yaku.Chittoi, new YakuInfo(Yaku.Chittoi, 2, Condition.MenzenOnly)},
            {Yaku.Chanta, new YakuInfo(Yaku.Chanta, 2, Condition.DecreaseHanWhenFuro)},
            {Yaku.Ittsu, new YakuInfo(Yaku.Ittsu, 2, Condition.DecreaseHanWhenFuro)},
            {Yaku.SamsaekDounsun, new YakuInfo(Yaku.SamsaekDounsun, 2, Condition.DecreaseHanWhenFuro)},

            // 3판역
            {Yaku.Ryanpeko, new YakuInfo(Yaku.Ryanpeko, 3, Condition.MenzenOnly, YakuSet(Yaku.Ipeko))},
            {Yaku.Junchan, new YakuInfo(Yaku.Junchan, 3, Condition.DecreaseHanWhenFuro, YakuSet(Yaku.Chanta))},
            {Yaku.Honiisou, new YakuInfo(Yaku.Honiisou, 3, Condition.DecreaseHanWhenFuro)},

            // 6판역
            {Yaku.Chiniisou, new YakuInfo(Yaku.Chiniisou, 6, Condition.DecreaseHanWhenFuro, YakuSet(Yaku.Honiisou))},

            // 12판
            {Yaku.Tenhou, new YakuInfo(Yaku.Tenhou, 12, Condition.MenzenOnly)},
            {Yaku.Chihou, new YakuInfo(Yaku.Chihou, 12, Condition.MenzenOnly)},
            {Yaku.Daisangen, new YakuInfo(Yaku.Daisangen, 12, Condition.FuroOK)},
            {Yaku.SsuAnkou, new YakuInfo(Yaku.SsuAnkou, 12, Condition.MenzenOnly)},
            {Yaku.Tsuuiisou, new YakuInfo(Yaku.Tsuuiisou, 12, Condition.FuroOK)},
            {Yaku.Ryuuiisou, new YakuInfo(Yaku.Ryuuiisou, 12, Condition.FuroOK)},
            {Yaku.Chinroutou, new YakuInfo(Yaku.Chinroutou, 12, Condition.FuroOK)},
            {Yaku.Kokushimusou, new YakuInfo(Yaku.Kokushimusou, 12, Condition.MenzenOnly)},
            {Yaku.Syosushi, new YakuInfo(Yaku.Syosushi, 12, Condition.FuroOK)},
            {Yaku.Daisushi, new YakuInfo(Yaku.Daisushi, 12, Condition.FuroOK)},
            {Yaku.Ssukanzu, new YakuInfo(Yaku.Ssukanzu, 12, Condition.FuroOK)},
            {Yaku.Chuuren, new YakuInfo(Yaku.Chuuren, 12, Condition.MenzenOnly)},

            // 도라네코 1판
            {Yaku.Dora, new YakuInfo(Yaku.Dora, 1, Condition.FuroOK)},
            {Yaku.AkaDora, new YakuInfo(Yaku.AkaDora, 1, Condition.FuroOK)},
            {Yaku.UraDora, new YakuInfo(Yaku.UraDora, 1, Condition.FuroOK)},
            {Yaku.NukiDora, new YakuInfo(Yaku.NukiDora, 1, Condition.FuroOK)},

        };


        static SortedSet<Yaku> YakuSet(params Yaku[] yakues)
        {
            SortedSet<Yaku> yakuSet = new SortedSet<Yaku>(yakues);
            return yakuSet;
        }

        private YakuInfo(Yaku yaku, int han, Condition condition, SortedSet<Yaku> ignoreYakues = null)
        {
            this.yaku = yaku;
            this.Han = han;
            this.condition = condition;
            lowerYakues = ignoreYakues ?? new SortedSet<Yaku>();
        }
    }


    public static partial class MahjongYakuSolver
    {
        //역 계산기

        public static bool Get1HanYakues(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            //SortedSet<Yaku> yakues;
            CheckRiichi(info, yakues);
            CheckTanyao(info, yakues);
            CheckTsumo(info, yakues);
            CheckTon(info, yakues);
            CheckNan(info, yakues);
            CheckSha(info, yakues);
            CheckPei(info, yakues);
            CheckHaku(info, yakues);
            CheckHatsu(info, yakues);
            CheckChun(info, yakues);
            CheckPinfu(info, yakues);
            CheckIpeko(info, yakues);
            CheckChanKan(info, yakues);
            CheckRinshan(info, yakues);
            CheckHaitei(info, yakues);
            CheckHoutei(info, yakues);
            CheckItpatsu(info, yakues);




            return true;
        }
        public static void Get2HanYakues(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            CheckDaburi(info, yakues);
            CheckSansokuDonggak(info, yakues);
            CheckSankan(info, yakues);
            CheckToiToi(info, yakues);
            CheckSanAnkou(info, yakues);
            CheckSyosangen(info, yakues);
            CheckHonnodou(info, yakues);
            CheckChittoi(info, yakues);
            CheckChanta(info, yakues);
            CheckIttsu(info, yakues);
            CheckSamsaekDongsun(info, yakues);

        }
        public static void Get3orHigherHanYakues(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            CheckRyanpeko(info, yakues);
            CheckJunchan(info, yakues);
            CheckHonisou(info, yakues);
            CheckChinisou(info, yakues);
        }
        public static void GetYakumanYakues(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            CheckTenhou(info, yakues);
            CheckChihou(info, yakues);
            CheckDaisangen(info, yakues);
            CheckSsuAnkou(info, yakues);
            CheckTsuuiisou(info, yakues);
            CheckChinroutou(info, yakues);
            CheckKokushimusou(info, yakues);
            CheckSushi(info, yakues);
            CheckSsuKanzu(info, yakues);
            CheckChuuren(info, yakues);
        }


        public static void IsSamSaek(MahjongWin winHand, out bool samsaekdongsun, out bool samsaekdonggak)
        {
            if(winHand.winType != MahjongWin.WinType.Normal)
            {
                samsaekdonggak = false;
                samsaekdongsun = false;
                return;
            }
            List<MahjongBlock> tmpBlocks;
            foreach(var i in winHand.bodies)
            {
                bool allSequence = false;
                bool alltriples = false;
                tmpBlocks = new List<MahjongBlock>(winHand.bodies);
                tmpBlocks.Remove(i);

                //모두 커쯔 또는 슌쯔가 아니라면 조건에 맞지 않아.
                int sequenceCount = tmpBlocks.Count(block => block.bodyType == MahjongBlock.BodyType.Sequence);
                if (sequenceCount == 3) allSequence = true;
                else if (sequenceCount == 0) alltriples = true;
                else continue;


                // 시작 번호가 모두 같아야겠지.
                if (tmpBlocks[0].startingNumber != tmpBlocks[1].startingNumber) continue;
                if (tmpBlocks[1].startingNumber != tmpBlocks[2].startingNumber) continue;

                // 만수, 삭수, 통수가 모두 있어야겠지.
                SortedSet<MahjongTile.TileType> tileTypeSet = new SortedSet<MahjongTile.TileType>();
                foreach(var item in tmpBlocks)
                {
                    //자패면 스킵. 
                    if (item.ContainsZapae) continue;
                    tileTypeSet.Add(item.tileType);
                }

                if (tileTypeSet.Count != 3) continue;

                samsaekdonggak = alltriples;
                samsaekdongsun = allSequence;
                return;



            }

            samsaekdonggak = false;
            samsaekdongsun = false;
            return;
        }
        
        public static int IpekoCounter(MahjongWin info)
        {
            
            int count = 0;
            if (info.winType != MahjongWin.WinType.Normal) return count;


            for(int i = 0; i < 3; i++)
            {
                if(info.bodies[i] == info.bodies[i + 1])
                {
                    count++;
                    i++;
                    continue;
                }
            }
            return count;
        }
        
        public static bool IsIttsu(MahjongWin winHand)
        {
            if(winHand.winType != MahjongWin.WinType.Normal)
            {
                return false;
            }
            List<MahjongBlock> tmpBlocks;
            SortedSet<int> ittsuSet = new SortedSet<int>() { 1, 4, 7 };
            foreach (var i in winHand.bodies)
            {
                tmpBlocks = new List<MahjongBlock>(winHand.bodies);
                tmpBlocks.Remove(i);

                //자패라면 스킵
                if (tmpBlocks.Count(block => block.ContainsZapae) > 0) continue;
                //전부 슌쯔여야 함
                if (tmpBlocks.Count(block => block.bodyType == MahjongBlock.BodyType.Sequence) != 3) continue;

                HashSet<MahjongTile.TileType> types = new HashSet<MahjongTile.TileType>();
                SortedSet<int> blockSet = new SortedSet<int>();
                foreach(var block in tmpBlocks)
                {
                    types.Add(block.tileType);
                    blockSet.Add(block[0].number);

                }

                if (types.Count > 1) continue;
                if (!blockSet.SetEquals(ittsuSet)) continue;
                return true;


            }
            return false;
        }
        
        public static bool IsHonisou(MahjongWin winHand)
        {
            if (winHand.winType == MahjongWin.WinType.Kokushimusou) return false;
            List<MahjongBlock> tmpBlocks = winHand.GetAllBlocks();
            HashSet<MahjongTile.TileType> types = new HashSet<MahjongTile.TileType>(); ;

            foreach(var block in tmpBlocks)
            {
                if (block.ContainsZapae) continue;
                types.Add(block.tileType);
            }
            return types.Count == 1;
        }
        
        public static bool IsChuuren(MahjongWin winHand)
        {
            if (winHand.winType != MahjongWin.WinType.Normal) return false;
            if (!winHand.IsHandConcealed) return false;

            List<MahjongTile> hand = winHand.GetAllTiles();

            string chuurenFormStr = "1m1m1m2m3m4m5m6m7m8m9m9m9m";
            StringBuilder sb = new StringBuilder(chuurenFormStr);
            MahjongTile.TileType chuurenType = hand[0].tileType;
            char manpingsak = chuurenType.ToString()[0];

            for(int i=0; i< sb.Length; i++)
            {
                if (sb[i] != 'm') continue;
                sb[i] = manpingsak;
            }
            List<MahjongTile> chuurenForm = MahjongTile.StringToTiles(sb.ToString());
            
            // 이제 어떻게 비교해야 할까?
            // 11123456779998(8 오름패)
            // 1112345678999

            foreach(var i in chuurenForm)
            {
                hand.Remove(i);
            }
            if(hand.Count == 1 && hand[0].tileType == chuurenType)
            {
                return true;
            }
            else
            {
                return false;
            }



        }
    


    }

    // 역 관리 - 1판역(IsTanYao 등)
    public static partial class MahjongYakuSolver
    {

        //리치
        static bool CheckRiichi(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isRiichi)
            {
                yakues.Add(Yaku.Riichi);
                return true;
            }
                return false;
        }
        //탕야오
        static bool CheckTanyao(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isKokushimushou) return false;
            if (info.bodyContains19 + info.headContains19 +
                info.bodyContainsZapae + info.headContainsZapae == 0)
            {
                yakues.Add(Yaku.Tanyao);
                return true;
            }

            return false;
        }
        //쯔모
        static bool CheckTsumo(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isHandMenzen && info.isWinTileTsumo)
            {
                yakues.Add(Yaku.Tsumo);
                return true;
            }

            return false;
        }

        //역패
        static bool CheckTon(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.doBodyContainsTon)
            {
                int count = 0;
                count += info.roundWind == Wind.Ton ? 1 : 0;
                count += info.seatWind == Wind.Ton ? 1 : 0;
                if (count == 2)
                    yakues.Add(Yaku.DoubleTon);
                else if(count == 1)
                    yakues.Add(Yaku.Ton);
                else
                {
                    return false;
                }
                return true;
            }

            return false;
        }
        static bool CheckNan(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.doBodyContainsNan)
            {
                int count = 0;
                count += info.roundWind == Wind.Nan ? 1 : 0;
                count += info.seatWind == Wind.Nan ? 1 : 0;
                if (count == 2)
                    yakues.Add(Yaku.DoubleNan);
                else if (count == 1)
                    yakues.Add(Yaku.Nan);
                else
                {
                    return false;
                }
                return true;
            }

            return false;
        }
        static bool CheckSha(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.doBodyContainsSha)
            {
                int count = 0;
                count += info.roundWind == Wind.Sha ? 1 : 0;
                count += info.seatWind == Wind.Sha ? 1 : 0;
                if (count == 2)
                    yakues.Add(Yaku.DoubleSha);
                else if (count == 1)
                    yakues.Add(Yaku.Sha);
                else
                {
                    return false;
                }
                return true;
            }

            return false;
        }
        static bool CheckPei(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.doBodyContainsPei)
            {
                int count = 0;
                count += info.roundWind == Wind.Pei ? 1 : 0;
                count += info.seatWind == Wind.Pei ? 1 : 0;
                if (count == 2)
                    yakues.Add(Yaku.DoublePei);
                else if (count == 1)
                    yakues.Add(Yaku.Pei);
                else
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        static bool CheckHaku(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.doBodyContainsHaku)
            {
                yakues.Add(Yaku.Haku);
                return true;
            }

            return false;
        }
        static bool CheckHatsu(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.doBodyContainsHatsu)
            {
                yakues.Add(Yaku.Hatsu);
                return true;
            }

            return false;
        }
        static bool CheckChun(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.doBodyContainsChun)
            {
                yakues.Add(Yaku.Chun);
                return true;
            }

            return false;
        }

        //[Obsolete("이 메서드는 아직 구현되지 않았으니 수정하세요!")]
        static bool CheckPinfu(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isPinfu)
            {
                yakues.Add(Yaku.Pinfu);
                return true;
            }

            return false;
        }

        static bool CheckIpeko(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.ipekoCount == 1)
            {
                yakues.Add(Yaku.Ipeko);
                return true;
            }

            return false;
        }
        static bool CheckChanKan(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isChanKan)
            {
                yakues.Add(Yaku.ChanKkan);
                return true;
            }

            return false;
        }
        static bool CheckRinshan(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isRinshan)
            {
                yakues.Add(Yaku.Rinshan);
                return true;
            }

            return false;
        }
        static bool CheckHaitei(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isHaitei)
            {
                yakues.Add(Yaku.Haitei);
                return true;
            }

            return false;
        }
        static bool CheckHoutei(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isHoutei)
            {
                yakues.Add(Yaku.Houtei);
                return true;
            }

            return false;
        }
        static bool CheckItpatsu(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isItpatsu)
            {
                yakues.Add(Yaku.Itpatsu);
                return true;
            }

            return false;
        }






        //템플런
        //static bool Check_(MahjongHandInfo info, SortedSet<Yaku> yakues)
        //{
        //    if (condition)
        //    {
        //        yakues.Add();
        //        return true;
        //    }

        //    return false;
        //}


    } 
    //2~6판역, 도라
    public static partial class MahjongYakuSolver
    {
        static bool CheckDaburi(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isDoubleRiichi)
            {
                yakues.Add(Yaku.DoubleRiichi);
                return true;
            }

            return false;
        }
        static bool CheckSansokuDonggak(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isSansokuToukou)
            {
                yakues.Add(Yaku.SansyokuDoko);
                return true;
            }

            return false;
        }

        static bool CheckSankan(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.kanCount == 3)
            {
                yakues.Add(Yaku.SanKkanzu);
                return true;
            }

            return false;
        }

        static bool CheckToiToi(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.tripleCount == 4)
            {
                yakues.Add(Yaku.ToiToi);
                return true;
            }

            return false;
        }
        static bool CheckSanAnkou(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.closedTripleCount == 3)
            {
                yakues.Add(Yaku.SanAnkou);
                return true;
            }

            return false;
        }
        static bool CheckSyosangen(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            int count = 0;
            count += info.doBodyContainsHaku ? 1 : 0;
            count += info.doBodyContainsHatsu ? 1 : 0;
            count += info.doBodyContainsChun ? 1 : 0;
            if (count == 2 && info.doHeadContainsDragonTile)
            {
                yakues.Add(Yaku.Syoshangen);
                return true;
            }

            return false;
        }

        static bool CheckHonnodou(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if ( info.bodyContainsOnly19 + info.bodyContainsZapae == 4 && info.headContains19 + info.headContainsZapae == 1
                || info.headContainsZapae + info.headContains19 == 7)
            {
                yakues.Add(Yaku.Honnodou);
                return true;
            }

            return false;
        }

        static bool CheckChittoi(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isChittoi)
            {
                yakues.Add(Yaku.Chittoi);
                return true;
            }

            return false;
        }

        
        static bool CheckChanta(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.bodyContains19 + info.bodyContainsZapae == 4 && info.headContains19 + info.headContainsZapae == 1
                || info.headContainsZapae + info.headContains19 == 7
                )
            {
                yakues.Add(Yaku.Chanta);
                return true;
            }

            return false;
        }

        static bool CheckIttsu(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isIttsu)
            {
                yakues.Add(Yaku.Ittsu);
                return true;
            }

            return false;
        }
        static bool CheckSamsaekDongsun(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isSamsaek)
            {
                yakues.Add(Yaku.SamsaekDounsun);
                return true;
            }

            return false;
        }

        // 
        // 3판역
        //

        static bool CheckRyanpeko(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.ipekoCount == 2)
            {
                yakues.Add(Yaku.Ryanpeko);
                return true;
            }

            return false;
        }

        static bool CheckJunchan(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.bodyContains19 == 4 && info.headContains19 == 1)
            {
                yakues.Add(Yaku.Junchan);
                return true;
            }

            return false;
        }
        static bool CheckHonisou(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isHonisou)
            {
                yakues.Add(Yaku.Honiisou);
                return true;
            }

            return false;
        }

        // 6판역
        static bool CheckChinisou(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isHonisou && info.bodyContainsZapae + info.headContainsZapae == 0)
            {
                yakues.Add(Yaku.Chiniisou);
                return true;
            }

            return false;
        }


        // 도라 이거 어쩔??

        static bool CheckDora(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.doraCount > 0)
            {
                yakues.Add(Yaku.Dora);
                return true;
            }

            return false;
        }
        static bool CheckUraDora(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.uradoraCount > 0 && info.isRiichi)
            {
                yakues.Add(Yaku.UraDora);
                return true;
            }

            return false;
        }
        static bool CheckAkaDora(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.akadoraCount > 0)
            {
                yakues.Add(Yaku.AkaDora);
                return true;
            }

            return false;
        }
        static bool CheckNukidora(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.nukidoraCount > 0)
            {
                yakues.Add(Yaku.NukiDora);
                return true;
            }

            return false;
        }


    }
    // 야꾸만
    public static partial class MahjongYakuSolver
    {
        static bool CheckTenhou(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isTenhou)
            {
                yakues.Add(Yaku.Tenhou);
                return true;
            }

            return false;
        }
        static bool CheckChihou(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isChihou)
            {
                yakues.Add(Yaku.Chihou);
                return true;
            }

            return false;
        }
        static bool CheckDaisangen(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {

            if (info.doBodyContainsHaku && info.doBodyContainsHatsu && info.doBodyContainsChun)
            {
                yakues.Add(Yaku.Daisangen);
                return true;
            }

            return false;
        }
        static bool CheckSsuAnkou(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.closedTripleCount == 4)
            {
                yakues.Add(Yaku.SsuAnkou);
                return true;
            }

            return false;
        }
        static bool CheckTsuuiisou(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isKokushimushou) return false;
            if (info.bodyContainsZapae == 4 && info.headContainsZapae == 1
                || info.headContainsZapae == 7)
            {
                yakues.Add(Yaku.Tsuuiisou);
                return true;
            }

            return false;
        }
        static bool CheckChinroutou(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.bodyContainsOnly19 == 4 && info.headContains19 == 1)
            {
                yakues.Add(Yaku.Chinroutou);
                return true;
            }

            return false;
        }
        static bool CheckKokushimusou(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isKokushimushou)
            {
                yakues.Add(Yaku.Kokushimusou);
                return true;
            }

            return false;
        }
        static bool CheckSushi(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            int count = 0;
            count += info.doBodyContainsTon ? 1 : 0;
            count += info.doBodyContainsNan ? 1 : 0;
            count += info.doBodyContainsSha ? 1 : 0;
            count += info.doBodyContainsPei ? 1 : 0;
            if (count == 3 && info.doHeadContainsWindTile)
            {
                yakues.Add(Yaku.Syosushi);
                return true;
            }
            else if(count == 4)
            {
                yakues.Add(Yaku.Daisushi);
                return true;
            }

            return false;
        }
        static bool CheckSsuKanzu(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.kanCount == 4)
            {
                yakues.Add(Yaku.Ssukanzu);
                return true;
            }

            return false;
        }
        static bool CheckChuuren(MahjongHandInfo info, SortedSet<Yaku> yakues)
        {
            if (info.isChuuren)
            {
                yakues.Add(Yaku.Chuuren);
                return true;
            }

            return false;
        }

    }

