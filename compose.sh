ver=1.0.0
docker build -t "scholtz2/algorand-asa-airdrop:$ver-stable" -f AirdropAlgorandASA/Dockerfile  ./
docker push "scholtz2/algorand-asa-airdrop:$ver-stable"
echo "Image: scholtz2/algorand-asa-airdrop:$ver-stable"
